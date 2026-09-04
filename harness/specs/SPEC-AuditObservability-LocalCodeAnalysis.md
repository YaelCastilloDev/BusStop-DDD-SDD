# SPEC-AuditObservability-LocalCodeAnalysis

## 1. Title and bounded context owner

Reproducible local SonarQube analysis for the backend and frontend.
Primary bounded context: **AuditObservability** (repository quality tooling).
Status: Gates 1-3 validated for implementation under the user's 2026-09-03 request.
Merge remains maintainer-controlled; verification evidence is required before merge.

References: `harness/system-design.md`, `harness/specs/clean-architecture-conventions.md`,
`harness/specs/lifecycle.md`, `harness/specs/gates-guardrails.md`, and the
`busstop-domain`, `clean-architecture`, and `busstop-harness` project skills.
The concurrent `SPEC-TransitCatalog-CreateRouteWithStops` feature is out of scope.

## 2. Problem statement and user impact

The current SonarQube script assumes an externally provisioned server and a Windows
self-hosted GitHub Actions runner. It imports only backend coverage. A contributor
cloning the repository cannot reproduce the server setup or obtain comparable
backend and frontend analysis using the repository alone.

Provide an explicitly started local Docker Compose stack and one documented
PowerShell 7 workflow that builds, tests, and analyzes both codebases on demand.
Keep ordinary CI intact and remove the obsolete SonarQube runner job and its setup
instructions. This change does not activate remote CI integrations or modify
application behavior.

## 3. Domain invariants and permissions

- Aggregate root: not applicable; this is developer tooling, not a domain feature.
- Route, Stop, and ModerationAction invariants, API behavior, and events stay intact.
- No application database, Keycloak account, or cross-context data writes occur.
- SonarQube owns a dedicated PostgreSQL database and persistent named Docker volumes.
- Publish SonarQube only on loopback by default; do not publish its database port.
- Pin SonarQube Community Build, PostgreSQL, and scanner versions; verify compatibility
  against current upstream documentation during implementation. Do not use `latest`.
- Use a separate Compose project; normal application start/stop does not manage it.
- Contributors with Docker access can start and stop their own stack. A local
  SonarQube administrator sets up the project and issues a project analysis token;
  analysts receive only the permissions needed for analysis.
- Keep tokens and database credentials out of Git, generated reports, and normal logs.
  Document scanner command-line token exposure to privileged local processes.
- Stop retains data. There is no automatic volume deletion, native-install removal,
  user-account migration, or purge of existing SonarQube history. Destructive reset
  remains a separately documented, explicit administrator action.

## 4. Use-case slice path

Operational slice: `scripts/sonarqube/` with a dedicated repository Compose file.
There is no `BusStop.UseCases` application slice: creating a command/handler/API for
developer tooling would introduce an unnecessary runtime dependency.

## 5. Layer file checklist

| Area | Planned files and responsibility |
| --- | --- |
| Core | No aggregate, specification, invariant, or domain dependency changes. |
| UseCases | No commands, queries, handlers, DTOs, or dependency changes. |
| Infrastructure | No runtime repository, service, or migration changes. Dedicated SonarQube/PostgreSQL Compose configuration at repository level. |
| Web | No endpoint, request DTO, validator, or DI changes. |
| Tooling | `scripts/sonarqube/Start-SonarQube.ps1`, `Stop-SonarQube.ps1`, and `Invoke-SonarQubeAnalysis.ps1`; shared helper if useful; non-secret environment example; pinned scanner in `.config/dotnet-tools.json`. |
| Frontend test configuration | `src/BusStop.Frontend/vite.config.ts` coverage reporter and source coverage scope; `package.json` or lockfile only if necessary for reproducible tooling. No UI/business logic edits. |
| CI and documentation | Remove only the stale `sonarqube` job from `.github/workflows/ci.yml`; replace `scripts/sonarqube/README.md`; update the repository entry-point documentation and ignore rules as needed. |
| Tests and evidence | Validate Compose, parse scripts, exercise prerequisite/failure checks, run existing Unit/Integration/Functional and frontend browser suites with coverage, then inspect imported analysis and stop/start persistence. Tooling-focused tests where needed. |

## 6. Command/query and endpoint impact

The start command validates Docker/Compose and local configuration, starts the
dedicated services, and waits a bounded time for SonarQube readiness. It must explain
port conflicts, server startup failures, and required host settings. Credentials
may be initialized into an ignored local file; repeated starts preserve them.

The stop command stops only the dedicated stack and preserves its volumes.

The analysis command resolves paths relative to the repository, supports PowerShell
7 on Windows/Linux/macOS, validates required tools/server/token, and uses one pinned
SonarScanner for .NET invocation for the monorepo. It must:

1. Restore the local scanner and repository dependencies reproducibly using the
   .NET 10 SDK and the frontend's pnpm lockfile.
2. Start scanning before a non-incremental Release build of the backend and its
   UnitTests, IntegrationTests, and FunctionalTests projects. Exclude the net9.0
   AspireTests project as existing CI does.
3. Run those three existing backend suites with fresh OpenCover and TRX reports.
   Integration and functional tests retain their existing PostGIS Testcontainers.
4. Run the frontend's existing headless Chromium/Vitest coverage command and emit
   LCOV. Include production frontend sources in coverage while keeping generated
   artifacts, dependencies, and test utilities appropriately excluded.
5. Import backend coverage/results and frontend LCOV into one project. Ensure
   supported frontend source is analyzed even though it is outside C# projects.
   Avoid duplicate test/source indexing and broad exclusions that hide application code.
6. Submit only after successful build/tests and verify expected fresh reports exist.
   Surface scanner errors and quality-gate failure with nonzero exit codes; explain
   in documentation that existing code quality issues can fail the gate.

Do not silently fall back to stale reports or a partial test set. If optional faster
analysis modes are provided, identify their reduced coverage and keep the complete
backend/frontend workflow as the default. Cleanup is limited to verified generated
paths inside this repository; source files and unrelated results are preserved.

No application endpoint or domain command/query contract changes. First-run
documentation covers Docker/Compose resources and host requirements, PowerShell 7,
.NET, Node/pnpm, Chromium installation, start, manual administrator password change,
project/token creation, analysis, dashboard access, and stop. Existing native server
users receive migration guidance without automatic deletion.

## 7. Event impact

Published domain/integration events: none. Consumed domain/integration events: none.
Analysis writes operational data only to the dedicated local SonarQube instance.
No application event dispatcher or message-broker configuration changes.

## 8. Acceptance criteria and validation matrix

| ID | Given / when / then | Evidence |
| --- | --- | --- |
| AC1 | Given a fresh clone and documented prerequisites, when the contributor runs start, then Compose starts pinned SonarQube and PostgreSQL services and the dashboard becomes available on loopback. | Compose configuration validation and runtime readiness smoke test. |
| AC2 | Given an initialized project, when stop and start run, then project settings and analysis history remain and ordinary application services are unaffected. | Dedicated project/volume inspection and persistence smoke test. |
| AC3 | Given first-run instructions, when an administrator creates the project and token, then a contributor can run analysis without editing tracked files or installing a GitHub runner. | Documentation walkthrough; ignored configuration/secret checks. |
| AC4 | Given healthy Docker and valid analysis credentials, when default analysis runs, then all three backend test suites generate fresh OpenCover/TRX and frontend browser tests generate fresh LCOV. | Existing test results, report existence/content checks, and command orchestration evidence. |
| AC5 | Given successful default analysis, when the project is inspected, then both C# and TypeScript/TSX sources appear and both backend and frontend coverage are imported. | Scanner import logs and SonarQube project measures/source inspection. |
| AC6 | Given a missing prerequisite, unavailable server, failed test, missing report, or failed quality gate, when analysis runs, then it reports an actionable failure with nonzero exit and does not claim success or reuse old coverage. | Focused failure-path checks and real run evidence where available. |
| AC7 | Given the updated repository, when tracked configuration/docs are searched, then the stale Windows self-hosted SonarQube job and native-server-only instructions are absent, while ordinary format/build/test jobs remain. | Scoped diff and reference search. |
| AC8 | Given any invocation directory or supported OS, when scripts resolve repository/report paths, then path handling is platform-neutral and destructive cleanup cannot escape the intended generated-output directory. | PowerShell parsing, path review, and targeted smoke checks; disclose platforms not executed. |

Preflight: Gate 1 passes with one AuditObservability owner. Gate 2 passes because
domain language is unchanged. Gate 3 passes because tooling stays outside application
layers and creates no dependencies. Gates 4 and 5 have no changed domain/API contracts;
review verifies that limitation. Gate 6 remains open until evidence covers the
applicable criteria. A broken Docker host must be reported as a runtime verification
blocker rather than a passing container or integration-test result.

## 9. Rollout and rollback considerations

Roll out as an opt-in local workflow with a documented first-run setup. Commit only
the template/configuration/scripts and documentation, never local credentials or
analysis state. Do not register runners, set GitHub secrets, or publish the server.
Document the difference between this reproducible developer setup and a shared
production SonarQube service, which requires separate availability, access, backup,
and upgrade planning.

Rollback the tooling/configuration change while leaving Docker volumes intact.
Database upgrades may prevent downgrading SonarQube against the same database;
back up PostgreSQL before changing server versions and restore a compatible backup
when required. Remove stale repository instructions/configuration within scope, but
leave native installations, unrelated dirty work, and existing user data untouched.

## Verification evidence (2026-09-03)

These results describe the implementation checkout, which also contained concurrent
uncommitted feature work. The isolated publication branch contains only this tooling
slice; its commit/push hooks provide separate validation of that checkout.

- Compose validation passed with the committed environment template. Official
  SonarQube 26.9.0.129388-community and PostgreSQL 17.11-bookworm image digests were
  resolved from Docker Hub; the scanner remains pinned at 11.3.0.
- All PowerShell files parsed successfully. The dependency-free tooling checks
  passed for URL/port validation, native argument/error forwarding, existing-volume
  credential protection, unchanged credentials on restart, explicit Compose project
  flags, and cleanup limited to the generated scanner directory.
- Backend UnitTests: 323 passed; OpenCover and TRX emitted. OpenCover reports 6,353
  sequence points, with 1,044 visited in this unit-only verification run. This is
  not the full combined coverage baseline.
- Frontend: 13 test files / 68 tests passed with V8 coverage. LCOV contains 79 source
  paths, all resolving within the repository. JSON resource import warnings were
  eliminated by excluding non-executable JSON from coverage. ESLint, TypeScript's
  config project check, and Prettier passed for the changed frontend configuration.
- Generated scanner state, local `.env`, reports, and database backups are ignored.
  The stale SonarQube CI job was removed; ordinary CI jobs were retained.
- Review identified two issues, both corrected and re-reviewed: new clones must
  reuse credentials for existing named volumes, and report-copy failure must still
  run scanner cleanup. No outstanding static review findings.
- **Runtime verification blocked:** Docker Desktop reports an inference-manager
  startup failure and its Linux engine does not respond. The new start command
  returned its intended actionable timeout after 20 seconds. No real SonarQube
  containers, server accounts, or analysis tokens were created during validation.
  Integration/Functional suites, live indexing and coverage imports, quality-gate
  handling, and stop/start history persistence remain unverified until Docker is
  repaired. Linux/macOS execution has not been performed. Gate 6 remains open;
  do not describe the full analysis workflow as runtime-verified or merge-ready.
