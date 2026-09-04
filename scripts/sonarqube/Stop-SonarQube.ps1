#Requires -Version 7.2
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')
Assert-SonarDocker
if (-not (Test-Path -LiteralPath $script:SonarEnvFile)) {
    throw 'scripts/sonarqube/.env is missing. Restore the existing configuration before managing this stack.'
}
Invoke-SonarCompose @('stop')
Write-Host 'SonarQube and its database are stopped. Projects, tokens, and analysis history remain in the named volumes.'
