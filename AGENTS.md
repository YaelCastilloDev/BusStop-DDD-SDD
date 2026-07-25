# AGENTS.md — BusStop Operational Cheat Sheet

## Quick start
```powershell
.\start-all.ps1                # Full stack (Docker + API + Frontend)
.\start-all.ps1 -SkipFrontend  # Backend only
.\start-all.ps1 -Build         # Rebuild containers first
docker compose down            # Stop all Docker services
```

| Service | Port | Notes |
|---|---|---|
| PostgreSQL (PostGIS) | 5432 | DB: `busstop`, user/pass: `busstop` |
| Keycloak | 8080 | Realm: `auth-demo`, admin/admin |
| RabbitMQ | 5672 (mgmt: 15672) | guest/guest |
| API (HTTPS) | 57679 | Swagger: `/swagger`, Scalar: `/scalar/v1` |
| Frontend | 5173 | Vite dev server |
| Aspire Dashboard | 18888 | OTLP telemetry |

Test users (password: `password`): `registered1`, `curator1`, `subadmin1`, `admin1`

## Everyday commands

### Backend (.NET 10)
```bash
dotnet build BusStop.slnx
dotnet test BusStop.slnx                                    # All tests
dotnet test BusStop.slnx --filter "FullyQualifiedName~TestName"  # Single test
dotnet test --settings .runsettings                         # With parallel config
dotnet format style --verify-no-changes --verbosity diagnostic  # CI format check
```

### EF Core migrations
```bash
# From src/BusStop.Web/ directory:
dotnet ef migrations add MigrationName -c AppDbContext \
  -p ../BusStop.Infrastructure/BusStop.Infrastructure.csproj \
  -s BusStop.Web.csproj -o Data/Migrations

dotnet ef database update -c AppDbContext \
  -p ../BusStop.Infrastructure/BusStop.Infrastructure.csproj \
  -s BusStop.Web.csproj
```
Migrations run automatically on startup in Development. In production, set `Database:ApplyMigrationsOnStartup: true`.

### Frontend (src/BusStop.Frontend)
```bash
pnpm install                  # pnpm, not npm
pnpm dev                      # Vite dev server
pnpm lint                     # ESLint
pnpm format:check             # Prettier check
pnpm format                   # Prettier write
pnpm knip                     # Dead code check
pnpm test                     # Vitest (browser mode via Playwright)
pnpm test:browser:install     # Install Chromium for Playwright (once)
pnpm build-theme              # Build Keycloakify theme
```

## Project map

```
Core ──► UseCases ──► Infrastructure ──► Web ──► AspireHost
                                                    ServiceDefaults (shared)
```

| Project | Purpose | Key tech |
|---|---|---|
| `BusStop.Core` | Domain: aggregates, VOs, specs, domain events | Ardalis.*, Mediator.Abstractions, Vogen |
| `BusStop.UseCases` | Application: CQRS handlers, DTOs | Mediator source-gen |
| `BusStop.Infrastructure` | Data access, external services | EF Core + PostGIS, MassTransit/RabbitMQ, Resend |
| `BusStop.Web` | REST API | FastEndpoints, JWT (Keycloak), Serilog, Scalar |
| `BusStop.AspireHost` | Orchestration | Aspire PostgreSQL + RabbitMQ hosting |
| `BusStop.ServiceDefaults` | Shared OTel + resilience | OpenTelemetry, health checks |
| `BusStop.Frontend` | React SPA | Vite, TanStack Router/Query, shadcn/ui, Tailwind v4, Keycloak JS |

**Tests:** UnitTests (NetArchTest + NSubstitute), IntegrationTests (Testcontainers PostGIS), FunctionalTests (WebApplicationFactory + Testcontainers), AspireTests (net9.0 only).

## Gotchas

- **pnpm, not npm.** The frontend lockfile is `pnpm-lock.yaml`. `start-all.ps1` calls `npm run dev` but pnpm must be used for installing.
- **Mediator source generator (v3), not MediatR.** Uses `IMediator`/`IRequestHandler<>` from `Mediator.Abstractions` with compile-time codegen via `Mediator.SourceGenerator`. Pipeline behaviors: `LoggingBehavior`, `CurrentUserBehavior`. (`DomainExceptionBehavior` is a safety net for the future, not actively relied on.)
- **Pre-release .NET SDK.** `global.json` pins `10.0.100` with `rollForward: latestMajor` and `allowPrerelease: true`. You may need a preview SDK.
- **PostGIS required.** PostgreSQL spatial extension is mandatory (even in Testcontainers: `postgis/postgis:15-3.3`). The `NearbyRoutesQueryService` performs spatial queries.
- **AspireTests targets net9.0.** Every other project targets `net10.0`. AspireTests cannot be built in a net10.0-only context.
- **FunctionalTests use Testcontainers PostgreSQL,** not the SQLite connection string in `appsettings.Testing.json`. The test factory (`CustomWebApplicationFactory`) overrides the connection string at runtime.
- **Central Package Management.** All package versions in `Directory.Packages.props`. Project files reference packages without `Version` attribute.
- **GitHub Copilot instructions are stale.** `.github/copilot-instructions.md` references `Clean.Architecture` (template name) and `net9.0`. Trust the actual code over that file.
- **SaveChanges() is blocked.** `AppDbContext.SaveChanges()` (sync) throws `NotSupportedException`. Only `SaveChangesAsync()` is valid.

## Layered conventions (cheat sheet)

| Layer | Rule |
|---|---|
| Core | `Guard.Against` in constructors for invariants; `Result<T>` from static `Create()` factory methods for expected failures. No EF/ASP.NET deps. Domain events past-tense (`RouteDeletedEvent`), inherit `DomainEventBase`. |
| UseCases | One command/query + handler per use case. Return `Result<T>`. No try-catch for expected failures. |
| Infrastructure | Specifications for non-trivial queries (no inline LINQ). Repos implement `IRepository<T>`/`IReadRepository<T>` from Core. |
| Web | One FastEndpoint file per operation (`Create.cs`, `Delete.cs`, etc.). Co-locate request DTO + validator. Every endpoint with input must have a `Validator<TRequest>`. Primary constructors assign to `_privateFields`. |
| Tests | `dotnet format --verify-no-changes` must pass. Architecture tests in `UnitTests/Architecture/` enforce layer boundaries, aggregate folder structure, pattern compliance, and endpoint validator requirements via NetArchTest. Functional tests use `WebApplicationFactory<Program>`. |

## Where rules live

- **Per-layer conventions:** `harness/specs/clean-architecture-conventions.md`, `domain.md`, `usecase.md`, `infrastructure.md`, `web.md`, `test.md`
- **Validation gates:** `harness/specs/gates-guardrails.md`
- **Domain glossary:** `harness/specs/domain.md` and `.cursor/rules/busstop-domain.mdc`
- **Agent definitions:** `.opencode/agents/` (planner, context, domain, usecase, infrastructure, web, reviewer, test, frontend)
- **Skills:** `.opencode/skills/` (busstop-domain, clean-architecture, csharp-core, csharp-web, frontend/*)
- **Cursor rules:** `.cursor/rules/` (18 .mdc files covering backend + frontend)
- **CI:** `.github/workflows/ci.yml` — format check then build + unit → integration → functional tests
