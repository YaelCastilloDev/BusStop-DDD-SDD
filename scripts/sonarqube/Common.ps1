#Requires -Version 7.2
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:SonarRepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$script:SonarComposeFile = Join-Path $PSScriptRoot 'compose.yml'
$script:SonarEnvFile = Join-Path $PSScriptRoot '.env'

function Invoke-SonarCommand {
    param(
        [Parameter(Mandatory)][string]$Command,
        [Parameter(Mandatory)][string[]]$Arguments
    )
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        # Do not print arguments: scanner begin/end contain the analysis token.
        throw "$Command failed (exit code $LASTEXITCODE). See the command output above."
    }
}

function Assert-SonarDocker {
    Get-Command docker -ErrorAction Stop | Out-Null
    $info = [Diagnostics.ProcessStartInfo]::new('docker')
    $info.ArgumentList.Add('info')
    $info.ArgumentList.Add('--format')
    $info.ArgumentList.Add('{{.OSType}}')
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(20000)) {
            $process.Kill($true)
            throw 'Docker did not respond within 20 seconds. Start or repair Docker Desktop / Docker Engine, then retry.'
        }
        if ($process.ExitCode -ne 0 -or $stdout.Result.Trim() -ne 'linux') {
            throw 'A running Linux Docker engine is required. Start Docker Desktop in Linux-container mode (or Docker Engine on Linux), then retry.'
        }
    }
    finally {
        $process.Dispose()
    }
    Invoke-SonarCommand docker @('compose', 'version', '--short')
}

function Get-SonarLocalUrl {
    $port = 9000
    if ($env:SONAR_PORT) {
        if (-not [int]::TryParse($env:SONAR_PORT, [ref]$port) -or $port -lt 1 -or $port -gt 65535) {
            throw 'SONAR_PORT must be an integer between 1 and 65535.'
        }
    }
    return "http://127.0.0.1:$port"
}

function Initialize-SonarEnvironment {
    if (-not (Test-Path -LiteralPath $script:SonarEnvFile)) {
        $volumes = @(Invoke-SonarCommand docker @('volume', 'ls', '--filter', 'name=busstop-sonarqube_postgres_data', '--format', '{{.Name}}'))
        if ($volumes -contains 'busstop-sonarqube_postgres_data') {
            throw 'This Docker host already has a SonarQube database. Copy scripts/sonarqube/.env from the original checkout (or restore its password) before starting this clone. Do not generate new credentials for an existing volume.'
        }
        $password = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
        New-Item -ItemType File -Path $script:SonarEnvFile -Value "SONAR_DB_PASSWORD=$password`n" | Out-Null
        if (-not $IsWindows) {
            Invoke-SonarCommand chmod @('600', $script:SonarEnvFile)
        }
        Write-Host "Created $script:SonarEnvFile with a random database password (ignored by Git)."
    }
    if ((Get-Content -LiteralPath $script:SonarEnvFile -Raw) -match 'replace-with-a-long-random-password') {
        throw 'Replace the database password placeholder in scripts/sonarqube/.env before starting.'
    }
}

function Invoke-SonarCompose {
    param([Parameter(Mandatory)][string[]]$Arguments)
    Invoke-SonarCommand docker (@('compose', '--project-name', 'busstop-sonarqube', '--env-file', $script:SonarEnvFile, '--file', $script:SonarComposeFile) + $Arguments)
}

function Remove-SonarScannerState {
    # The .NET scanner leaves MSBuild hooks active while this generated directory
    # exists, including after a failed build. Never clean arbitrary caller paths.
    $scannerDirectory = [IO.Path]::GetFullPath((Join-Path $script:SonarRepositoryRoot '.sonarqube'))
    if ([IO.Path]::GetDirectoryName($scannerDirectory) -ne $script:SonarRepositoryRoot) {
        throw 'Refusing to clean scanner state outside the repository.'
    }
    if (Test-Path -LiteralPath $scannerDirectory) {
        $item = Get-Item -LiteralPath $scannerDirectory -Force
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Refusing to clean a linked .sonarqube directory.'
        }
        Remove-Item -LiteralPath $scannerDirectory -Recurse -Force
    }
}
