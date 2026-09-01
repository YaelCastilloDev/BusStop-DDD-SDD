---
name: pre-commit-guard
description: Diagnostic-only skill for BusStop pre-commit and pre-push hook failures. When a hook blocks a commit or push, this skill diagnoses the failure and offers solutions but NEVER auto-fixes code.
---

# Pre-Commit & Pre-Push Guard

This skill activates when a pre-commit or pre-push hook failure is detected, or when the user asks about a blocked commit or push.
It is **diagnostic only** — it never modifies code unless the user explicitly authorizes it.

## When to Use
- User says 'pre-commit failed', 'hook blocked my commit', or 'commit rejected'
- User says 'pre-push failed', 'hook blocked my push', or 'push rejected'
- User says 'format check failed', 'unit tests failed at commit', or 'tests broke'
- User runs `git commit` or `git push` and gets an error from Husky.Net

## The Pre-Commit Pipeline

Runs on every `git commit`. Defined in `.husky/task-runner.json`:

| Order | Task | What it checks | Failure type |
|---|---|---|---|
| 1 | `nuget-restore` | `dotnet restore -p:NuGetAudit=false` | NuGet restore errors (package resolution) |
| 2 | `format-check` | `dotnet format style --verify-no-changes --verbosity diagnostic --no-restore` | Code style violations per `.editorconfig` |
| 3 | `unit-tests` | `dotnet test tests/BusStop.UnitTests/BusStop.UnitTests.csproj --verbosity quiet` | Failing unit tests (xUnit v3, NSubstitute) |

Only **UnitTests** run on pre-commit. No Docker required.

## The Pre-Push Pipeline

Runs on every `git push`. Defined in `.husky/task-runner.json`:

| Order | Task | What it checks | Failure type |
|---|---|---|---|
| 1 | `nuget-restore` | `dotnet restore -p:NuGetAudit=false` | NuGet restore errors |
| 2 | `integration-tests` | `dotnet test tests/BusStop.IntegrationTests/BusStop.IntegrationTests.csproj --verbosity quiet` | EF Core mappings, repository + specification against real DB (Testcontainers PostGIS) |
| 3 | `functional-tests` | `dotnet test tests/BusStop.FunctionalTests/BusStop.FunctionalTests.csproj --verbosity quiet` | HTTP routes via WebApplicationFactory + Testcontainers PostGIS |

**Docker is required** for pre-push. Both integration and functional tests use Testcontainers (`postgis/postgis:15-3.3`) and IntegrationTests also uses RabbitMQ (`rabbitmq:3-management`).

## Diagnostic Procedure

### Step 1: Identify which task failed

Read the Husky.NET output. The last line before the failure shows which task failed:
```
❌ Task 'format-check' failed in 18,367ms
❌ Task 'unit-tests' failed in 5,234ms
```

### Step 2: Diagnose by failure type

#### Format check failure (`format-check`)

**Symptoms:**
- Output contains `error IMPORTS:`, `error IDE####:`, or `error WHITESPACE:`
- Shows a count like `10 of 302 files formatted`
- Lists specific file paths with line numbers

**Diagnosis procedure:**
1. Identify all files listed with formatting errors from the hook output
2. Read the specific lines referenced (e.g., `StopConfiguration.cs(1,1)` means line 1, column 1)
3. Classify each violation:
   - `IMPORTS` — `using` statements are in wrong order (must be alphabetically sorted, `System.*` first)
   - `IDE0161` — namespace should use file-scoped style (`namespace Foo;` not `namespace Foo { }`)
   - `WHITESPACE` — trailing whitespace, inconsistent indentation, or missing final newline
   - Other `IDE####` — run `dotnet format style --verify-no-changes --verbosity diagnostic 2>&1 | rg "error IDE"` to see full list

**Recommendation present to user:**
- `dotnet format style` will auto-fix all style violations at once
- Or fix specific files manually based on the IDE code shown

#### Unit test failure (`unit-tests`)

**Symptoms:**
- Output contains `Failed:` with a count greater than 0
- Shows individual test failure details (method name, assertion message, stack trace)

**Diagnosis procedure:**
1. Identify the failing test project and test class from the output
2. Read the failing test method to understand what it asserts
3. Look for recent code changes that may have broken the assertion
4. Run the failing test individually for more detail:
   ```
   dotnet test tests/BusStop.UnitTests/BusStop.UnitTests.csproj --filter "FullyQualifiedName~FailingTestName" --verbosity detailed
   ```

**Recommendation present to user:**
- State exactly which test failed, what it expected vs. what it got
- Identify the likely cause (changed API, modified entity, altered business rule)
- Suggest the fix location (but DO NOT implement it)

#### NuGet restore failure (`nuget-restore`)

**Symptoms:**
- Output contains `error NU####:` or `Failed to restore`
- Usually a package resolution error or network issue

**Diagnosis procedure:**
1. Read the NuGet error code (e.g., NU1101, NU1102)
2. Check `Directory.Packages.props` for the affected package
3. Verify the package version exists on NuGet.org

**Recommendation present to user:**
- For NU1902 (vulnerability): this is suppressed in the hook via `-p:NuGetAudit=false`, so it shouldn't appear. If it does, verify the restore command in `task-runner.json`.
- For other NU errors: suggest running `dotnet restore` manually to see full error details

#### Integration test failure (`integration-tests`) — pre-push only

**Symptoms:**
- Husky output shows `❌ Task 'integration-tests' failed`
- Failing tests typically involve EF Core mappings, repository queries, or data persistence
- Tests use Testcontainers PostGIS and RabbitMQ

**Common causes:**
1. **Docker not running.** Testcontainers cannot spin up PostGIS or RabbitMQ. Verify with `docker ps`.
2. **Port conflicts.** Testcontainers assigns dynamic ports. If parallel execution is on, multiple containers may collide.
3. **Migration mismatch.** A new entity or property is not reflected in EF Core configuration in `Data/Config/`.
4. **Specification logic error.** A `Specification<T>` in Core has incorrect filter logic.

**Diagnosis procedure:**
1. Verify Docker is running: `docker ps` (must show running containers)
2. Run the failing test individually for detailed output:
   ```
   dotnet test tests/BusStop.IntegrationTests/BusStop.IntegrationTests.csproj --filter "FullyQualifiedName~FailingTestName" --verbosity detailed
   ```
3. Check the EF Core configuration file for the affected entity in `src/BusStop.Infrastructure/Data/Config/`
4. Verify the specification in `src/BusStop.Core/{Entity}Aggregate/Specifications/`

**Recommendation present to user:**
- State which test failed and what assertion failed
- If Docker-related, suggest starting Docker Desktop
- If EF Core mapping issue, identify the specific entity and missing/invalid configuration
- DO NOT implement the fix

#### Functional test failure (`functional-tests`) — pre-push only

**Symptoms:**
- Husky output shows `❌ Task 'functional-tests' failed`
- Tests involve HTTP request/response via `WebApplicationFactory<Program>`
- Tests use Testcontainers PostGIS for the database

**Common causes:**
1. **Docker not running.** Same as integration tests.
2. **Endpoint returns wrong status code.** A handler or validator changed the response.
3. **Authorization failure.** JWT or policy change broke an authenticated endpoint.
4. **Request/response DTO mismatch.** A property was renamed, added, or removed.

**Diagnosis procedure:**
1. Verify Docker is running
2. Run the failing test individually:
   ```
   dotnet test tests/BusStop.FunctionalTests/BusStop.FunctionalTests.csproj --filter "FullyQualifiedName~FailingTestName" --verbosity detailed
   ```
3. Check the FastEndpoints endpoint file in `src/BusStop.Web/Endpoints/`
4. Check the validator in the same directory
5. Check the handler in `src/BusStop.UseCases/`

**Recommendation present to user:**
- State the HTTP status code and response body the test received vs. expected
- Identify the endpoint, handler, and validator files involved
- Suggest which layer likely contains the bug
- DO NOT implement the fix

#### Docker-related failure (pre-push)

**Symptoms:**
- Error messages mentioning `Docker.DotNet.DockerApiException`, `container not found`, or timeout
- Testcontainers cannot pull or start the required images

**Diagnosis procedure:**
1. Check Docker is running: `docker ps`
2. Check required images are available: `docker images postgis/postgis:15-3.3` and `docker images rabbitmq:3-management`
3. If images are missing, Testcontainers will pull them automatically on first run — this adds time but only once

**Recommendation present to user:**
- Start Docker Desktop if not running
- Wait for image pull to complete on first run (may take 1-2 minutes)
- If persistent, check Docker disk space and network connectivity

## Critical Rule: Diagnose and Recommend — Never Auto-Fix

### Mandate

When a hook fails, you **MUST**:

1. **Describe the problem.** State clearly what went wrong: which task failed, which files are affected, which tests broke, what error messages were produced. Do not skip or summarize — give the user the full picture.

2. **Offer specific solutions.** After describing each problem, present the fix. For format violations, tell them exactly which command to run. For test failures, identify the root cause and which files to change. For Docker issues, tell them the exact troubleshooting steps.

### This skill is DIAGNOSTIC ONLY. The agent MUST NOT fix anything unless the user explicitly authorizes it.

**DO NOT:**
- Run `dotnet format style` to auto-fix formatting
- Modify any `.cs` file to fix test failures
- Change any project configuration or package references
- Run `git commit --no-verify` or `git push --no-verify` as a workaround

**ONLY:**
1. Read the error output and affected files
2. Classify the failure as format, test, or restore
3. Present a diagnostic summary to the user
4. Offer specific, actionable recommendations

**The user must explicitly say one of these to proceed with fixes:**
- 'implement it'
- 'fix it'
- 'go ahead'
- 'proceed'
- 'apply the fix'
- 'yes, fix the formatting'
- 'run dotnet format'

## Bypass Information (Informational Only)

The pre-commit hook can be bypassed with `git commit --no-verify`. The pre-push hook can be bypassed with `git push --no-verify`. These instructions are provided for awareness only — never suggest them unless the user asks.
