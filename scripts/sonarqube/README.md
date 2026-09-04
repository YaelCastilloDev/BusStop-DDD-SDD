# Local SonarQube analysis

Analyze the backend and frontend together, on demand. Every contributor runs the
same versioned SonarQube Community Build/PostgreSQL containers and pinned scanner.
No GitHub runner, native SonarQube installation, or paid license is required.
Run the examples from the repository root in PowerShell 7 (`pwsh`). Scripts also
resolve paths correctly when invoked from another directory.

## Prerequisites (once per machine)

- Docker Desktop in Linux-container mode on Windows/macOS, or Docker Engine on
  Linux, with Compose v2 supporting `up --wait`. Docker must be running.
- PowerShell 7.2+, the .NET SDK selected by `global.json` (.NET 10), Node.js 24 LTS
  (minimum 22.13), and pnpm 11.19.0. pnpm is pinned in the frontend `packageManager`
  field; see [pnpm installation](https://pnpm.io/installation) if needed.
- Several GB of free disk space. Give Docker at least 4 GB RAM for SonarQube;
  6-8 GB is a practical starting allocation when also running test containers.
  Builds and browser tests need additional host memory.

Install Chromium and its Linux system libraries once:

```powershell
pnpm --dir src/BusStop.Frontend install --frozen-lockfile
pnpm --dir src/BusStop.Frontend test:browser:install
```

Elasticsearch requires `vm.max_map_count >= 524288` and `fs.file-max >= 131072`
on the Linux host or Docker Desktop VM. Compose sets container file/thread limits;
it cannot set these host kernel limits. On Linux, check with
`sysctl vm.max_map_count fs.file-max`. On Windows/WSL2:

```powershell
wsl -d docker-desktop -u root sysctl vm.max_map_count fs.file-max
# Only if the map count is too low:
wsl -d docker-desktop -u root sysctl -w vm.max_map_count=524288
```

Persist insufficient Linux settings in `/etc/sysctl.d/99-sonarqube.conf` and apply
with `sudo sysctl --system`. Docker Desktop VM settings may need reapplying after
a restart. See the [official host requirements](https://hub.docker.com/_/sonarqube).
Elasticsearch's bootstrap checks remain enabled.

## First start

```powershell
pwsh ./scripts/sonarqube/Start-SonarQube.ps1
```

Start creates `scripts/sonarqube/.env` with a random database password if absent,
starts the dedicated `busstop-sonarqube` Compose project, and waits for readiness.
The first image pull may take several minutes. `.env` is ignored by Git; retain it
when restarting the existing database. `.env.example` supports manual Compose setup.
For a second clone/worktree on the same Docker host, copy `.env` from the original
checkout. Start refuses to generate a different password for an existing volume.

1. Open [the dashboard](http://127.0.0.1:9000), sign in with `admin` / `admin`, and
   change the administrator password. Store it in your password manager.
2. Create a **local project**, project key `busstop`, display name `BusStop`.
   Keep the built-in Sonar way quality gate and select a New Code definition
   appropriate for your work, such as the last 30 days.
3. In **My Account > Security**, generate a **Project Analysis Token** for `busstop`.
   Choose an expiry and save it in your password manager; it is shown only once.

The dashboard binds only to `127.0.0.1`; the database port is not published.
For a different port set `$env:SONAR_PORT = '9001'` in the PowerShell session used
for both start and analysis. Keep port overrides in the environment, not `.env`.

## Analyze whenever you want

```powershell
pwsh ./scripts/sonarqube/Invoke-SonarQubeAnalysis.ps1
```

The script starts the containers if needed and prompts for the token with hidden
input. To reuse a token within the current PowerShell session:

```powershell
$env:SONAR_TOKEN = Read-Host 'Project analysis token' -MaskInput
pwsh ./scripts/sonarqube/Invoke-SonarQubeAnalysis.ps1
Remove-Item Env:SONAR_TOKEN # Clear when finished.
```

Each run restores frozen pnpm dependencies and the pinned .NET scanner, ensures
Chromium is installed, and runs all frontend Vitest browser tests with V8 coverage.
It then starts the .NET scanner, performs non-incremental Release builds, and runs
UnitTests, IntegrationTests, and FunctionalTests with OpenCover coverage and TRX.
Existing PostGIS/RabbitMQ Testcontainers fixtures are isolated from application
data; the normal application stack does not need to be started.

One analysis uploads both codebases and both coverage reports to
[the BusStop project](http://127.0.0.1:9000/dashboard?id=busstop). The .NET scanner's
multi-language mode includes TS/TSX outside `.csproj` files and recognizes
`*.test.*` / `*.spec.*` frontend test files. No second scanner/project is needed.

Fresh reports live in `TestResults/SonarQube/<run-id>/`: backend OpenCover/TRX and
frontend LCOV/HTML. Unique directories prevent stale coverage imports. Build/test
failures stop before submission. Generated `.sonarqube` state is cleaned after
scanning, including failures, so analysis hooks and credentials do not linger.
A lock prevents overlapping scans in a checkout; avoid simultaneous builds there.

The main application projects and dependencies are built. AspireHost and the
separate net9.0 AspireTests are outside this workflow, matching ordinary CI scope.
The frontend's existing coverage exclusions (UI primitives, assets, route files)
remain; those files still receive static analysis. Eligible untested frontend
source now appears as 0% covered. Generated files, declarations, bundled Keycloak
development resources, dependencies, and build outputs are excluded from source
analysis. Exact scope: `SonarQube.Analysis.xml` and the frontend `vite.config.ts`.

By default, analysis waits up to five minutes for the quality gate and exits
nonzero on failure. A red gate is a code-quality result, not necessarily a setup
error: inspect the dashboard. `-NoQualityGateWait` uploads without awaiting the
gate; it does not skip tests or weaken the gate. `-ProjectKey another-key` selects
a different existing project.

Community Build supports one main branch per project. Scanning another Git branch
against the same project replaces its current view; use different local project
keys for independent baselines. PR decoration and branch analysis need a separately
designed edition/integration setup.

## Stop and inspect

```powershell
pwsh ./scripts/sonarqube/Stop-SonarQube.ps1
pwsh ./scripts/sonarqube/Start-SonarQube.ps1
docker compose --env-file scripts/sonarqube/.env -f scripts/sonarqube/compose.yml ps
docker compose --env-file scripts/sonarqube/.env -f scripts/sonarqube/compose.yml logs --tail 100
```

Stop preserves named volumes, accounts, tokens, settings, and analysis history.
It may wait for an active server analysis to finish. The application's
`start-all.ps1` / `stop-all.ps1` operate independently. Clones on the same Docker
host share `busstop-sonarqube`; each developer machine has its own instance.
Do not run `down -v` or prune these volumes unless intentionally discarding all data.

## Maintenance and old configuration

Official server/database images are pinned by version and digest; update both
together after checking the supported upgrade path. The scanner remains pinned in
`.config/dotnet-tools.json`. Back up PostgreSQL before upgrades; reverting an image
does not downgrade the database. Example backup while the server is stopped:

```powershell
$compose = @('compose', '--env-file', 'scripts/sonarqube/.env', '-f', 'scripts/sonarqube/compose.yml')
docker @compose stop sonarqube
docker @compose exec -T db pg_dump -U sonarqube -d sonarqube -Fc -f /tmp/sonarqube.dump
docker @compose cp db:/tmp/sonarqube.dump ./sonarqube-backup.dump
docker @compose start sonarqube
```

Keep backups and `.env` securely outside Git. The setup does not delete a native
installation or migrate its history. If one occupies port 9000, stop it yourself
or use `SONAR_PORT`. Do not attach a new server to an old database without first
checking versions and the supported migration path.

The obsolete Windows self-hosted SonarQube CI job was removed. If you previously
created a dedicated GitHub runner or `SONAR_HOST_URL`, `SONAR_PROJECT_KEY`, and
`SONAR_TOKEN` GitHub settings solely for it, retire those separately in GitHub.
Ordinary format/build/test CI continues unchanged. Localhost is unreachable from
a GitHub-hosted runner. For a future shared server, use `-SkipStart` and
`-HostUrl https://your-server`, providing a scoped token through CI secrets.
Shared production hosting also needs TLS, access control, scheduled backups, and
availability planning; this Compose stack is a local developer setup.

The .NET scanner requires the token on both begin/end command lines. Scripts do
not echo or save it, but privileged local processes can inspect arguments. The
scanner provisions its Java runtime; no native Java installation is required.

## Troubleshooting

- Docker timed out: start/repair Docker Desktop or Engine first. The preflight
  fails after 20 seconds instead of hanging indefinitely.
- Server unhealthy: check logs for Docker memory, port conflicts, host kernel
  limits, or database credentials that no longer match the persistent volume.
- Chromium cannot launch on Linux: rerun `test:browser:install` to install system
  libraries. Daily analysis installs browser binaries only.
- Authentication failed: check project key, token scope, and expiry. Supply
  `SONAR_TOKEN` for non-interactive CI; never commit it.
- Tests failed: resolve the reported code/test failure, then rerun. Partial
  reports remain locally but are not submitted as a complete analysis.
- Frontend coverage: inspect `frontend/lcov.info`; `SF:` paths are relative to
  the repository root, matching the monorepo scanner.

## Upstream references

Tooling maintainers can run the helper regression checks without a Docker daemon:
`pwsh ./scripts/sonarqube/Test-SonarQubeTooling.ps1`. These use isolated test files
and mocked Docker calls; they do not replace a real startup, scan, and persistence test.

- [Official Docker image and persistence](https://hub.docker.com/_/sonarqube)
- [Supported databases](https://docs.sonarsource.com/sonarqube-community-build/server-installation/installing-the-database)
- [.NET multi-language scanner](https://docs.sonarsource.com/sonarqube-community-build/analyzing-source-code/scanners/dotnet/configuring)
- [JavaScript/TypeScript LCOV import](https://docs.sonarsource.com/sonarqube-community-build/analyzing-source-code/test-coverage/javascript-typescript-test-coverage)
- [Vitest coverage configuration](https://vitest.dev/config/coverage.html)
