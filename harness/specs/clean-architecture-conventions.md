# Clean Architecture Conventions (Ardalis Template)

## Purpose
Define the structural, naming, and pattern rules agents must follow when generating or modifying BusStop code.
Based on the [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture) solution template for ASP.NET Core.

## Core Principles
- **Dependency rule:** dependencies point inward only. Core has zero infrastructure or UI references.
- **Separation of concerns:** business logic is independent of EF Core, HTTP, and third-party SDKs.
- **Dependency inversion:** Core/UseCases define abstractions; Infrastructure implements them.
- **Single responsibility:** one handler, specification, or endpoint class per orchestration flow.

## Solution Layout
```text
BusStop.slnx
├── src/
│   ├── BusStop.Core/             # Domain: aggregates, value objects, events, domain interfaces
│   ├── BusStop.UseCases/         # Application: CQRS commands, queries, handlers, DTOs
│   ├── BusStop.Infrastructure/   # EF Core, repositories, external integrations
│   ├── BusStop.Web/              # FastEndpoints API, middleware, DI startup hooks
│   ├── BusStop.AspireHost/       # .NET Aspire orchestration (optional)
│   └── BusStop.ServiceDefaults/  # Aspire shared defaults (OpenTelemetry, resilience)
└── tests/
    ├── BusStop.UnitTests/        # Domain and isolated logic
    ├── BusStop.IntegrationTests/ # DbContext, specifications, migrations
    └── BusStop.FunctionalTests/  # API route tests via WebApplicationFactory
```

## Layer Rules

### BusStop.Core
- **References:** `Ardalis.SharedKernel`, `Ardalis.GuardClauses`, `Ardalis.Specification`, `Ardalis.Result`, `Mediator.Abstractions` (for domain event handlers only). No EF Core or ASP.NET Core.
- **Organization:** one folder per aggregate root, not a flat `Entities/` folder.
```text
RouteAggregate/
├── Route.cs
├── RouteId.cs
├── RouteName.cs
├── Events/
│   └── RouteCreatedEvent.cs
├── Handlers/
│   └── RouteCreatedAuditHandler.cs
└── Specifications/
    └── RouteByIdSpec.cs
```
- **Aggregate roots:** inherit `EntityBase<T, TId>` and `IAggregateRoot`.
- **IDs:** Vogen `[ValueObject<T>]` structs (e.g., `RouteId`), not raw `Guid` or `int`.
- **Value objects:** co-located in aggregate folder. Use `ValueObject` base or records with `From()` factory methods.
- **Domain events:** folder `Events/`, past-tense names (e.g., `RouteUpdatedEvent`), inherit `DomainEventBase`.
- **Domain event handlers:** folder `Handlers/`, implement `INotificationHandler<TEvent>` from Mediator.
- **Specifications:** folder `Specifications/` inside each aggregate. Use `Ardalis.Specification`.
- **Invariants:** enforce with `Ardalis.GuardClauses` in constructors and factory methods.
- **Interfaces:** repository and service abstractions defined here, implemented in Infrastructure.

### BusStop.UseCases
- **References:** `BusStop.Core` only (+ Mediator, Ardalis.Result).
- **Organization:** feature slices, not technical folders.
```text
UseCases/Routes/Create/
├── CreateRouteCommand.cs
└── CreateRouteHandler.cs
```
- **Commands:** `<Action><Entity>Command` (e.g., `CreateRouteCommand`).
- **Queries:** `Get<Entity>Query`, `List<Entities>Query`.
- **Handlers:** `<CommandOrQueryName>Handler`, implementing `ICommandHandler<,>` or query equivalents from **Mediator** (not MediatR).
- **Returns:** use `Ardalis.Result` / `Result<T>` for expected failures (not found, validation), not exceptions.
- **Validators:** not in UseCases by default. Input validation belongs in the Web layer (FastEndpoints). Handlers may perform defensive checks for domain-level concerns.
- **Query services:** read-optimized queries via interfaces like `IListRoutesQueryService` defined here, implemented in Infrastructure.

### BusStop.Infrastructure
- **References:** `BusStop.Core`, `BusStop.UseCases`.
- **DbContext:** `AppDbContext` in `Data/`.
- **Configurations:** `<Entity>Configuration.cs` in `Data/Config/`.
- **Repositories:** `EfRepository<T>` extends `RepositoryBase<T>`; accept specifications, no ad-hoc LINQ in custom methods.
- **Query services:** implementations in `Data/Queries/`.
- **Domain events:** dispatch after successful `SaveChanges` via `MediatorDomainEventDispatcher`.

### BusStop.Web
- **References:** `BusStop.UseCases`; Infrastructure referenced only for DI registration.
- **Endpoints:** REPR pattern via **FastEndpoints** (not Controllers).
- **Endpoint classes:** operation names — `Create.cs`, `GetById.cs`, `List.cs`, `Update.cs`, `Delete.cs`.
- **Request/response DTOs:** co-located in endpoint folder (e.g., `Create.CreateRequest.cs`) or nested in endpoint file for small features.
- **Validators:** FastEndpoints `Validator<TRequest>` classes co-located with endpoints (e.g., `Create.CreateValidator.cs`).
- **Configurations:** `Configurations/MediatorConfig.cs`, `ServiceConfigs.cs`, `MiddlewareConfig.cs`, `LoggerConfigs.cs`.
- **Result mapping:** use `ResultExtensions` to map `Result<T>` to typed HTTP responses.
- **Primary constructors:** assign dependencies to private `_fields` (never use constructor parameters directly).

## Key Patterns
- **Guard clauses:** `Guard.Against.NullOrEmpty(value, nameof(value))` instead of manual null throws.
- **Specification:** all conditional queries as `Specification<T>` classes in Core, not inline repository logic.
- **Domain events:** aggregates call `RegisterDomainEvent`; handlers implement `INotificationHandler<T>`.
- **Result wrapper:** handlers return `Result<T>`; endpoints map results to HTTP responses without throwing for flow control.
- **Mediator:** source-generated `IMediator` for command/query dispatch. Register in `MediatorConfig.cs`.

## BusStop Domain Mapping
| Concept | Core location | Use case slice example |
|---------|---------------|------------------------|
| Route | `RouteAggregate/Route.cs` | `Routes/Create`, `Routes/Update`, `Routes/SoftDelete` |
| Stop | `StopAggregate/Stop.cs` | `Stops/Create`, `Stops/GetByLocation` |
| Contribution | `ContributionAggregate/Contribution.cs` | `Contributions/Submit` |
| ModerationAction | `ModerationActionAggregate/ModerationAction.cs` | `Moderation/Review`, `Moderation/Undo` |

> **Note:** The template ships with a `ContributorAggregate` as a reference vertical slice. Replace it incrementally with BusStop aggregates per approved feature specs.

## Project Coding Standards (BusStop)
- C# latest, primary constructors for DI in services/handlers/endpoints.
- Assign primary constructor parameters to private `_fields` for clarity and testability.
- Explicit typing; use `var` only when type is obvious.
- Types `internal sealed` by default unless extension requires otherwise.
- Null checks: `is null` / `is not null`.
- Async I/O: always `async`/`await` with `Async` suffix on method names.

## Testing Expectations
1. **UnitTests:** domain entities, value objects, specifications, use case handlers. Mock all externals. xUnit + NSubstitute + Shouldly.
2. **IntegrationTests:** EF configurations, specification query accuracy. EF Core InMemory or Testcontainers.
3. **FunctionalTests:** full HTTP route tests via `WebApplicationFactory` + `HttpClient`. Depends on Web project.

## Agent Non-Negotiables
- Never add EF Core, HTTP, or ASP.NET Core references to Core.
- Never put business logic in Web or Infrastructure layers.
- Never create repository methods with inline LINQ filters — use Specifications.
- Never skip FastEndpoints validators for endpoints that accept external input.
- Never use MediatR — use Mediator source generator.
- Every new use case gets: command/query, handler, endpoint with validator, and tests.
