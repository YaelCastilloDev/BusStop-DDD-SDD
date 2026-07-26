param(
    [switch]$SkipFrontend,
    [switch]$SkipApi,
    [switch]$SkipDocker,
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$dockerComposeEnv = Join-Path $projectRoot ".env"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  BusStop - Start All Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── 1. Prerequisites ──────────────────────────────────────
Write-Host "[1/5] Checking prerequisites..." -ForegroundColor Yellow

if (-not $SkipDocker) {
    try {
        docker info *>$null
        if ($LASTEXITCODE -ne 0) { throw "Docker not responding" }
        Write-Host "  Docker     : OK" -ForegroundColor Green
    } catch {
        Write-Host "  Docker     : NOT RUNNING - Start Docker Desktop first" -ForegroundColor Red
        exit 1
    }
}

if (-not $SkipFrontend) {
    try {
        node --version *>$null
        Write-Host "  Node.js    : OK ($(node --version))" -ForegroundColor Green
    } catch {
        Write-Host "  Node.js    : NOT FOUND" -ForegroundColor Red
        exit 1
    }
}

if (-not $SkipApi) {
    try {
        dotnet --version *>$null
        Write-Host "  .NET SDK   : OK ($(dotnet --version))" -ForegroundColor Green
    } catch {
        Write-Host "  .NET SDK   : NOT FOUND (API won't start)" -ForegroundColor Yellow
    }
}

# ── 2. Root .env file ─────────────────────────────────────
Write-Host ""
Write-Host "[2/5] Creating root .env for docker-compose..." -ForegroundColor Yellow

if (-not (Test-Path $dockerComposeEnv)) {
    @"
DB_USER=busstop
DB_PASSWORD=busstop
KEYCLOAK_ADMIN_USER=admin
KEYCLOAK_ADMIN_PASSWORD=admin
DOMAIN=localhost
RABBITMQ_USER=guest
RABBITMQ_PASS=guest
"@ | Set-Content -LiteralPath $dockerComposeEnv -Encoding UTF8
    Write-Host "  Created .env with default credentials" -ForegroundColor Green
} else {
    Write-Host "  .env already exists" -ForegroundColor Green
}

# ── 3. Docker Compose ─────────────────────────────────────
Write-Host ""
Write-Host "[3/5] Starting Docker services..." -ForegroundColor Yellow

if (-not $SkipDocker) {
    Push-Location $projectRoot
    try {
        if ($Build) {
            docker compose build
        }
        docker compose up -d
        Write-Host "  Docker services started" -ForegroundColor Green
    } finally {
        Pop-Location
    }
}

# ── 3b. Create busstop database ───────────────────────────
Write-Host ""
Write-Host "  Ensuring busstop database exists..." -ForegroundColor Yellow

$maxPgRetries = 30
$pgReady = $false
for ($i = 0; $i -lt $maxPgRetries; $i++) {
    $result = docker exec keycloak-db psql -U busstop -t -c "SELECT 1" 2>&1
    if ("$result" -match "1") { $pgReady = $true; break }
    Write-Host "  Waiting for PostgreSQL... ($($i+1)/$maxPgRetries)" -ForegroundColor Gray
    Start-Sleep -Seconds 2
}
if ($pgReady) {
    $dbExists = docker exec keycloak-db psql -U busstop -t -c "SELECT 1 FROM pg_database WHERE datname='busstop'" 2>&1
    if ("$dbExists" -notmatch "1") {
        docker exec keycloak-db psql -U busstop -c "CREATE DATABASE busstop;"
        Write-Host "  busstop database created" -ForegroundColor Green
    } else {
        Write-Host "  busstop database already exists" -ForegroundColor Green
    }
} else {
    Write-Host "  WARNING: PostgreSQL not ready — busstop DB will be created by EF migrations" -ForegroundColor Yellow
}

# ── 4. Wait for Keycloak ──────────────────────────────────
Write-Host ""
Write-Host "[4/5] Waiting for Keycloak to be ready..." -ForegroundColor Yellow

$maxRetries = 60
$retry = 0
$keycloakReady = $false

do {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/realms/auth-demo" -TimeoutSec 3 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            $keycloakReady = $true
        }
    } catch {
        # Still waiting
    }
    if (-not $keycloakReady) {
        $retry++
        Write-Host "  Waiting... ($retry/$maxRetries)" -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
} while (-not $keycloakReady -and $retry -lt $maxRetries)

if ($keycloakReady) {
    Write-Host "  Keycloak is ready!" -ForegroundColor Green
} else {
    Write-Host "  WARNING: Keycloak did not respond in time. It may still be starting." -ForegroundColor Yellow
}

# ── 5. Start API and Frontend ─────────────────────────────
Write-Host ""
Write-Host "[5/5] Starting application services..." -ForegroundColor Yellow
Write-Host ""

$apiProject = Join-Path $projectRoot "src\BusStop.Web\BusStop.Web.csproj"
$frontendDir = Join-Path $projectRoot "src\BusStop.Frontend"

if (-not $SkipApi -and (Test-Path $apiProject)) {
    Write-Host "  Starting API (dotnet run)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'BusStop API'; dotnet run --project `"$apiProject`" --launch-profile https" 
    Write-Host "    API will start on https://localhost:57679" -ForegroundColor Gray
} else {
    Write-Host "  API: SKIPPED (use -SkipApi or dotnet SDK not found)" -ForegroundColor Yellow
}

if (-not $SkipFrontend -and (Test-Path $frontendDir)) {
    Write-Host "  Starting Frontend (npm run dev)..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'BusStop Frontend'; Set-Location -LiteralPath `"$frontendDir`"; npm run dev"
    Write-Host "    Frontend will start on http://localhost:5173" -ForegroundColor Gray
} else {
    Write-Host "  Frontend: SKIPPED (use -SkipFrontend or Node.js not found)" -ForegroundColor Yellow
}

# ── Summary ───────────────────────────────────────────────
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Services Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PostgreSQL      : localhost:5432" -ForegroundColor White
Write-Host "  RabbitMQ        : localhost:5672 (mgmt: http://localhost:15672)" -ForegroundColor White
Write-Host "  Keycloak        : http://localhost:8080" -ForegroundColor White
Write-Host "  Keycloak Admin  : http://localhost:8080/admin" -ForegroundColor White
Write-Host "  API (swagger)   : https://localhost:57679/swagger" -ForegroundColor White
Write-Host "  Frontend        : http://localhost:5173" -ForegroundColor White
Write-Host "  Aspire Dashboard: http://localhost:18888" -ForegroundColor White
Write-Host ""
Write-Host "  Realm         : auth-demo" -ForegroundColor White
Write-Host "  Test users (password: password):" -ForegroundColor White
Write-Host "    registered1 / curator1 / subadmin1 / admin1" -ForegroundColor Gray
Write-Host ""
Write-Host "  To stop all Docker services:" -ForegroundColor Yellow
Write-Host "    docker compose down" -ForegroundColor White
Write-Host ""
