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
- **Aggregate roots:** inherit `EntityBase<TId>` and `IAggregateRoot`.
- **IDs:** `Ardalis.SharedKernel.ValueObject` subclasses (e.g., `RouteId`), not raw `Guid` or `int`.
- **Value objects:** co-located in aggregate folder. Use `ValueObject` base or records with `From()` factory methods.
- **Domain events:** folder `Events/`, past-tense names (e.g., `RouteUpdatedEvent`), inherit `DomainEventBase`.
- **Domain event handlers:** folder `Handlers/`, implement `INotificationHandler<TEvent>` from Mediator.
- **Specifications:** folder `Specifications/` inside each aggregate. Use `Ardalis.Specification`.
- **Invariants:** enforce domain rules with `DomainValidationException` in constructors and factory methods. Use `Ardalis.GuardClauses` for defensive null/range checks on non-domain code (configuration, pipeline behaviors, service constructors).
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
- **Returns:** use `Ardalis.Result` / `Result<T>` for expected failures (not found, validation). Handlers must NOT contain try-catch for domain exceptions — the `DomainExceptionBehavior` Mediator pipeline catches `DomainValidationException` and wraps in `Result<T>.Error()`. Do not let domain exceptions propagate to the API.
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
- **Domain exceptions:** throw `DomainValidationException` for domain invariant violations in entity factories and methods. Use `Guard.Against.*` for defensive null/range checks in non-domain code (configuration, pipeline behaviors).
- **Specification:** all conditional queries as `Specification<T>` classes in Core, not inline repository logic.
- **Domain events:** aggregates call `RegisterDomainEvent`; handlers implement `INotificationHandler<T>`.
- **Result wrapper:** handlers return `Result<T>`; endpoints map results to HTTP responses without throwing for flow control.
- **Mediator:** source-generated `IMediator` for command/query dispatch. Register in `MediatorConfig.cs`.

## Error Handling Strategy (Three-Layer Approach)

BusStop uses a three-layer error handling strategy combining typed domain exceptions, a Mediator pipeline behavior, and a global ASP.NET Core exception handler.

### Layer 1: Web — FluentValidation (FastEndpoints)
- Input-bearing endpoints MUST have a `Validator<TRequest>`.
- Catches malformed input before it reaches the handler or domain layer.
- Returns 400 with structured validation errors.

### Layer 2: Mediator Pipeline — DomainExceptionBehavior
- A single `IPipelineBehavior<,>` registered in `MediatorConfig.cs`.
- Catches `DomainValidationException` thrown by domain entities/value objects.
- Converts to `Result<T>.Error()` or `Result.Error()` automatically.
- Handlers contain ZERO try-catch for domain exceptions — the pipeline handles it.
- Does NOT catch `ArgumentException` or `Exception` — only `DomainValidationException`.

### Layer 3: ASP.NET Core — GlobalExceptionHandler
- Implements `IExceptionHandler`, registered via `services.AddExceptionHandler<T>()`.
- Catches any exception that escapes the Mediator pipeline (programming bugs, infrastructure failures).
- Returns RFC 7807 Problem Details JSON with 500 status.
- Logs full stack trace. Never leaks internals to the client.

### Two-Tool Strategy for Throwing Code

| Tool | Where | Purpose | HTTP Result |
|------|-------|---------|-------------|
| `DomainValidationException` | Domain entities/value objects | Business rule violations | 400 via pipeline |
| `Guard.Against.*` | Infrastructure, config, pipeline behaviors | Defensive programming bugs | 500 via global handler |

### Flow
```
Client Request
  │
  ▼
FastEndpoints Validator (FluentValidation) → 400 on malformed input
  │ (clean data)
  ▼
Mediator Pipeline
  ├── DomainExceptionBehavior ◄── catches DomainValidationException → Result.Error()
  ├── LoggingBehavior
  └── CurrentUserBehavior
  │
  ▼
UseCase Handler → calls domain factory (no try-catch needed)
  │
  ▼
Domain Entity → throw new DomainValidationException(...) for business rules
  │
  ▼
Repository → persistence
  │
  └── Any other exception → GlobalExceptionHandler → 500 ProblemDetails
```

### Example
```csharp
// Core — domain entity: throws DomainValidationException for business rules
public static Route Create(string name, long createdById)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new DomainValidationException("Route name is required.", nameof(name));
    if (createdById <= 0)
        throw new DomainValidationException("CreatedById must be positive.", nameof(createdById));
    return new Route(new RouteName(name), new UserId(createdById));
}

// UseCases — handler: clean, no try-catch (pipeline handles it)
public sealed class CreateRouteHandler(IRepository<Route> repository, ...)
  : ICommandHandler<CreateRouteCommand, Result<RouteResponse>>
{
    public async ValueTask<Result<RouteResponse>> Handle(
        CreateRouteCommand request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Sub))
            return Result<RouteResponse>.Unauthorized("Authentication required.");

        var route = Route.Create(request.Name, user.Id); // may throw DomainValidationException
        var created = await repository.AddAsync(route, ct);
        return new RouteResponse(created.Id, ...);
    }
}

// Infrastructure — config validation: Guard.Against for defensive checks
Guard.Against.Null(connectionString); // throws at startup if misconfigured
```

## BusStop Domain Mapping
| Concept | Core location | Use case slice example |
|---------|---------------|------------------------|
| Route | `RouteAggregate/Route.cs` | `Routes/Create`, `Routes/Update`, `Routes/SoftDelete` |
| Stop | `StopAggregate/Stop.cs` | `Stops/Create`, `Stops/GetByLocation` |
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
