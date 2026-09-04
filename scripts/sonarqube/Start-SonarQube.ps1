#Requires -Version 7.2
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')
$localUrl = Get-SonarLocalUrl
Assert-SonarDocker
Initialize-SonarEnvironment

Write-Host 'Starting SonarQube and its dedicated PostgreSQL database. The first image pull can take several minutes.'
try {
    Invoke-SonarCompose @('up', '--detach', '--wait', '--wait-timeout', '360')
}
catch {
    throw "SonarQube could not start. Check Docker resources, whether $localUrl is already in use, and the host settings in scripts/sonarqube/README.md. Inspect logs with: docker compose --env-file scripts/sonarqube/.env -f scripts/sonarqube/compose.yml logs --tail 100"
}
Write-Host "SonarQube is ready at $localUrl. First start: sign in with admin/admin, change the password, and create the busstop project and a project analysis token."
