#Requires -Version 7.2
# Dependency-free helper regression checks. No Docker daemon or SonarQube needed.
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')

function Assert-Tooling {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Assert-ToolingThrows {
    param([scriptblock]$Action, [string]$Expected)
    $caught = $null
    try { & $Action } catch { $caught = $_.Exception.Message }
    Assert-Tooling ($null -ne $caught -and $caught.Contains($Expected)) "Expected error containing: $Expected; actual: $caught"
}

$originalPort = $env:SONAR_PORT
$originalCommand = ${function:Invoke-SonarCommand}
$testDirectory = Join-Path $script:SonarRepositoryRoot ('TestResults/SonarQube/tooling-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null

try {
    $env:SONAR_PORT = $null
    Assert-Tooling ((Get-SonarLocalUrl) -eq 'http://127.0.0.1:9000') 'Default loopback URL changed.'
    $env:SONAR_PORT = '9001'
    Assert-Tooling ((Get-SonarLocalUrl) -eq 'http://127.0.0.1:9001') 'Port override was ignored.'
    $env:SONAR_PORT = '65536'
    Assert-ToolingThrows { Get-SonarLocalUrl } 'between 1 and 65535'

    # Exercise real native forwarding, including arguments that PowerShell could
    # otherwise interpret as switches, spaces, or statement separators.
    $values = @('path with spaces', '--logger:trx;LogFileName=tests.trx', '--', '-c')
    $forwarded = Invoke-SonarCommand node (@('-e', 'process.stdout.write(JSON.stringify(process.argv.slice(1)))', '--') + $values) | ConvertFrom-Json
    Assert-Tooling (($forwarded -join '|') -eq ($values -join '|')) 'Native arguments were changed.'
    Assert-ToolingThrows { Invoke-SonarCommand node @('-e', 'process.exit(7)') } 'exit code 7'

    # Mock only Docker/permission commands; all test files stay in this run's
    # ignored output directory, never in the actual local server configuration.
    $script:SonarRepositoryRoot = $testDirectory
    $script:SonarEnvFile = Join-Path $testDirectory '.env'
    $script:ExistingTestVolume = $true
    $script:CapturedArguments = @()
    function Invoke-SonarCommand {
        param([string]$Command, [string[]]$Arguments)
        $script:CapturedArguments = $Arguments
        if ($Command -eq 'docker' -and $Arguments[0] -eq 'volume' -and $script:ExistingTestVolume) {
            'busstop-sonarqube_postgres_data'
        }
    }

    Assert-ToolingThrows { Initialize-SonarEnvironment } 'original checkout'
    Assert-Tooling (-not (Test-Path -LiteralPath $script:SonarEnvFile)) 'A second clone generated incompatible credentials.'
    $script:ExistingTestVolume = $false
    Initialize-SonarEnvironment
    $firstEnvironment = Get-Content -LiteralPath $script:SonarEnvFile -Raw
    Assert-Tooling ($firstEnvironment -match '^SONAR_DB_PASSWORD=[0-9A-F]{64}\s*$') 'Fresh database password is missing or malformed.'
    $script:ExistingTestVolume = $true
    Initialize-SonarEnvironment
    Assert-Tooling ((Get-Content -LiteralPath $script:SonarEnvFile -Raw) -eq $firstEnvironment) 'Restart rotated existing credentials.'

    Invoke-SonarCompose @('up', '--detach', '--wait', '--wait-timeout', '360')
    Assert-Tooling ($script:CapturedArguments -contains 'busstop-sonarqube') 'Compose project is not isolated.'
    Assert-Tooling (($script:CapturedArguments | Select-Object -Last 5) -join ' ' -eq 'up --detach --wait --wait-timeout 360') 'Compose flags were not forwarded.'

    $scannerPath = Join-Path $testDirectory '.sonarqube'
    New-Item -ItemType Directory -Path $scannerPath | Out-Null
    New-Item -ItemType File -Path (Join-Path $scannerPath 'generated-state.txt') -Value 'test-generated state' | Out-Null
    Remove-SonarScannerState
    Assert-Tooling (-not (Test-Path -LiteralPath $scannerPath)) 'Scanner cleanup left generated state.'
    Assert-Tooling (Test-Path -LiteralPath $script:SonarEnvFile) 'Scanner cleanup removed unrelated files.'

    Write-Host 'Tooling checks passed: URL validation, native arguments/errors, existing-volume guard, credential persistence, Compose isolation, scoped scanner cleanup.'
}
finally {
    $env:SONAR_PORT = $originalPort
    ${function:Invoke-SonarCommand} = $originalCommand
}
