#Requires -Version 7.2
[CmdletBinding()]
param(
    [string]$HostUrl = $env:SONAR_HOST_URL,
    [string]$ProjectKey = 'busstop',
    [string]$Token = $env:SONAR_TOKEN,
    [switch]$SkipStart,
    [switch]$NoQualityGateWait
)

. (Join-Path $PSScriptRoot 'Common.ps1')

foreach ($command in @('dotnet', 'node', 'pnpm')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "$command is required. See scripts/sonarqube/README.md for clone setup."
    }
}
if ([string]::IsNullOrWhiteSpace($ProjectKey)) {
    throw 'ProjectKey must match an existing SonarQube project (default: busstop).'
}
if ([string]::IsNullOrWhiteSpace($HostUrl)) {
    $HostUrl = Get-SonarLocalUrl
}
$serverUri = $null
if (-not [Uri]::TryCreate($HostUrl, [UriKind]::Absolute, [ref]$serverUri) -or $serverUri.Scheme -notin @('http', 'https')) {
    throw 'HostUrl must be an absolute HTTP or HTTPS URL.'
}
if (-not $SkipStart -and $HostUrl.TrimEnd('/') -ne (Get-SonarLocalUrl)) {
    throw 'Use -SkipStart when analyzing against an externally managed SONAR_HOST_URL.'
}
$HostUrl = $HostUrl.TrimEnd('/')

if ($SkipStart) {
    Assert-SonarDocker # Existing integration/functional tests need Testcontainers.
}
else {
    & (Join-Path $PSScriptRoot 'Start-SonarQube.ps1')
}
try {
    $status = Invoke-RestMethod -Uri "$HostUrl/api/system/status" -TimeoutSec 10
}
catch {
    throw "Cannot reach SonarQube at $HostUrl. Start it and verify SONAR_HOST_URL."
}
if ($status.status -ne 'UP') {
    throw "SonarQube is not ready (status: $($status.status)). Check the server logs."
}
if ([string]::IsNullOrWhiteSpace($Token)) {
    if ($env:CI) {
        throw 'Set SONAR_TOKEN to a project analysis token for non-interactive analysis.'
    }
    $Token = Read-Host "Paste a project analysis token for $ProjectKey (input is hidden)" -MaskInput
}
if ([string]::IsNullOrWhiteSpace($Token)) {
    throw 'An analysis token is required. Create one in SonarQube > My Account > Security.'
}
try {
    $authentication = Invoke-RestMethod -Uri "$HostUrl/api/authentication/validate" -Headers @{ Authorization = "Bearer $Token" } -TimeoutSec 10
}
catch {
    throw 'SonarQube rejected the authentication request. Check the server URL and token.'
}
if (-not $authentication.valid) {
    throw 'SonarQube token is invalid or expired. Generate a new project analysis token.'
}

$frontendDirectory = Join-Path $script:SonarRepositoryRoot 'src/BusStop.Frontend'
$resultsRoot = Join-Path $script:SonarRepositoryRoot 'TestResults/SonarQube'
New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null
$analysisLock = $null
$scannerStarted = $false
Push-Location $script:SonarRepositoryRoot
try {
    try {
        $analysisLock = [IO.File]::Open((Join-Path $resultsRoot 'analysis.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    }
    catch {
        throw 'Another SonarQube analysis is using this checkout. Wait for it to finish.'
    }
    # Unique output prevents importing old or partially overwritten coverage.
    $runId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $relativeResults = "TestResults/SonarQube/$runId"
    $resultsDirectory = Join-Path $script:SonarRepositoryRoot $relativeResults
    $frontendResults = Join-Path $resultsDirectory 'frontend'
    New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null
    Write-Host "Reports for this run: $resultsDirectory"

    Invoke-SonarCommand dotnet @('tool', 'restore')
    Invoke-SonarCommand pnpm @('--dir', $frontendDirectory, 'install', '--frozen-lockfile')
    Invoke-SonarCommand pnpm @('--dir', $frontendDirectory, 'exec', 'playwright', 'install', 'chromium')
    Invoke-SonarCommand pnpm @('--dir', $frontendDirectory, 'test:coverage', '--coverage.reportsDirectory', $frontendResults)
    $lcov = Join-Path $frontendResults 'lcov.info'
    if (-not (Test-Path -LiteralPath $lcov) -or (Get-Item -LiteralPath $lcov).Length -eq 0) {
        throw 'Frontend tests did not produce LCOV. Analysis has not been submitted.'
    }

    $projects = @(
        @{ Path = 'tests/BusStop.UnitTests/BusStop.UnitTests.csproj'; Name = 'unit' },
        @{ Path = 'tests/BusStop.IntegrationTests/BusStop.IntegrationTests.csproj'; Name = 'integration' },
        @{ Path = 'tests/BusStop.FunctionalTests/BusStop.FunctionalTests.csproj'; Name = 'functional' }
    )
    foreach ($project in $projects) {
        Invoke-SonarCommand dotnet @('restore', $project.Path)
    }

    $gateWait = (-not $NoQualityGateWait).ToString().ToLowerInvariant()
    Remove-SonarScannerState # Clear stale generated state; also rejects linked paths before begin.
    $scannerStarted = $true
    $beginArguments = @(
        'tool', 'run', 'dotnet-sonarscanner', 'begin', "/k:$ProjectKey",
        "/s:$(Join-Path $PSScriptRoot 'SonarQube.Analysis.xml')",
        "/d:sonar.projectBaseDir=$script:SonarRepositoryRoot",
        "/d:sonar.host.url=$HostUrl", "/d:sonar.token=$Token",
        "/d:sonar.cs.opencover.reportsPaths=$relativeResults/backend/**/coverage.opencover.xml",
        "/d:sonar.cs.vstest.reportsPaths=$relativeResults/backend/**/*.trx",
        "/d:sonar.javascript.lcov.reportPaths=$relativeResults/frontend/lcov.info",
        "/d:sonar.qualitygate.wait=$gateWait", '/d:sonar.qualitygate.timeout=300'
    )
    Invoke-SonarCommand dotnet $beginArguments

    # C# analysis requires compilation after begin; cached builds are insufficient.
    Invoke-SonarCommand dotnet @('build', 'src/BusStop.Web/BusStop.Web.csproj', '-c', 'Release', '--no-restore', '--no-incremental')
    foreach ($project in $projects) {
        Invoke-SonarCommand dotnet @('build', $project.Path, '-c', 'Release', '--no-restore', '--no-incremental')
        $projectResults = Join-Path $resultsDirectory "backend/$($project.Name)"
        Invoke-SonarCommand dotnet @('test', $project.Path, '-c', 'Release', '--no-build', '--no-restore', '--verbosity', 'normal', '--logger:trx;LogFileName=tests.trx', '--results-directory', $projectResults, '--collect:XPlat Code Coverage', '--', 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover')
        $coverage = @(Get-ChildItem -LiteralPath $projectResults -Filter 'coverage.opencover.xml' -Recurse -File)
        if ($coverage.Count -eq 0 -or @($coverage | Where-Object Length -EQ 0).Count -gt 0 -or -not (Test-Path -LiteralPath (Join-Path $projectResults 'tests.trx'))) {
            throw "The $($project.Name) suite did not produce OpenCover/TRX. Analysis has not been submitted."
        }
    }

    Invoke-SonarCommand dotnet @('tool', 'run', 'dotnet-sonarscanner', 'end', "/d:sonar.token=$Token")
    $dashboardUrl = "$HostUrl/dashboard?id=$([Uri]::EscapeDataString($ProjectKey))"
    if ($NoQualityGateWait) {
        Write-Host "Analysis submitted; quality gate not awaited. Results: $dashboardUrl"
    }
    else {
        Write-Host "Analysis complete and quality gate passed: $dashboardUrl"
    }
}
finally {
    try {
        if ($scannerStarted) {
            try {
                $taskReport = Join-Path $script:SonarRepositoryRoot '.sonarqube/out/.sonar/report-task.txt'
                if (Test-Path -LiteralPath $taskReport) {
                    Copy-Item -LiteralPath $taskReport -Destination $resultsDirectory
                }
            }
            finally {
                Remove-SonarScannerState
            }
        }
    }
    finally {
        if ($null -ne $analysisLock) {
            $analysisLock.Dispose()
        }
        Pop-Location
    }
}
