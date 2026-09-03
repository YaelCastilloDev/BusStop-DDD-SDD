[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiExecutablePattern = Join-Path $projectRoot 'src\BusStop.Web\bin\*\BusStop.Web.exe'
$frontendDirectory = Join-Path $projectRoot 'src\BusStop.Frontend'

function Stop-BusStopProcessOnPort {
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [int]$Port,

        [Parameter(Mandatory)]
        [scriptblock]$IsExpectedProcess
    )

    $connections = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue
    $processIds = @($connections | Select-Object -ExpandProperty OwningProcess -Unique)

    if ($processIds.Count -eq 0) {
        Write-Host "  $Name     : not running" -ForegroundColor DarkGray
        return
    }

    foreach ($processId in $processIds) {
        $process = Get-CimInstance Win32_Process -Filter "ProcessId = $processId"

        if ($null -eq $process) {
            continue
        }

        if (-not (& $IsExpectedProcess $process)) {
            Write-Warning "Skipped PID $processId on port $Port because it is not a BusStop $Name process."
            continue
        }

        Stop-Process -Id $processId -Force
        Write-Host "  $Name     : stopped (PID $processId)" -ForegroundColor Green
    }
}

Write-Host '========================================' -ForegroundColor Cyan
Write-Host '  BusStop - Stop All Services' -ForegroundColor Cyan
Write-Host '========================================' -ForegroundColor Cyan
Write-Host ''

Write-Host '[1/2] Stopping local application services...' -ForegroundColor Yellow

Stop-BusStopProcessOnPort -Name 'API' -Port 57679 -IsExpectedProcess {
    param($process)

    $process.Name -eq 'BusStop.Web.exe' -and
        $process.ExecutablePath -like $apiExecutablePattern
}

Stop-BusStopProcessOnPort -Name 'Frontend' -Port 5173 -IsExpectedProcess {
    param($process)

    $process.Name -in @('node', 'node.exe') -and
        $process.CommandLine -like "*$frontendDirectory*node_modules\vite\*"
}

Write-Host ''
Write-Host '[2/2] Stopping Docker Compose services...' -ForegroundColor Yellow

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warning 'Docker CLI is not available. Docker services were not stopped.'
} else {
    docker info *>$null

    if ($LASTEXITCODE -ne 0) {
        Write-Warning 'Docker is not running. Docker services were not stopped.'
    } else {
        Push-Location $projectRoot
        try {
            docker compose down
            if ($LASTEXITCODE -ne 0) {
                throw 'docker compose down failed.'
            }

            Write-Host '  Docker Compose services stopped (volumes preserved).' -ForegroundColor Green
        } finally {
            Pop-Location
        }
    }
}

Write-Host ''
Write-Host 'BusStop services have been stopped.' -ForegroundColor Green
