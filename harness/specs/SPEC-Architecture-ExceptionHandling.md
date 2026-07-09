# SPEC-Architecture-ExceptionHandling

## Spec ID
`SPEC-Architecture-ExceptionHandling`

## Bounded Context
Cross-cutting — applies to all bounded contexts (TransitCatalog, IdentityAccess, NotificationContext, SearchReadModel, AuditObservability).

## Problem
The codebase requires a standardized, industry-standard approach to error handling. Two distinct categories of errors require different handling:
1. **Domain rule violations** (user-facing: invalid input, invariant breach) — should produce a 400 response.
2. **Programming/configuration bugs** (null references, missing config, network failure) — should produce a 500 response.

Ad-hoc try-catch in every handler creates boilerplate. A pipeline approach centralizes error handling without runtime overhead on the success path.

## Design

### Two-Tool Strategy

BusStop uses **two distinct tools** for two distinct jobs:

| Tool | Purpose | Throws | Caught By | HTTP |
|------|---------|--------|-----------|------|
| `Guard.Against.*` | Defensive programming (prevent bugs) | `ArgumentException`, `ArgumentNullException` | `GlobalExceptionHandler` | 500 |
| `DomainValidationException` | Domain rule violation (business logic) | Custom, typed exception | `DomainExceptionBehavior` pipeline | 400 |

This distinction is critical: `DomainExceptionBehavior` catches **only** `DomainValidationException`. If it caught `ArgumentException`, it would swallow real programming bugs (dictionary key not found, invalid cast) and return 400 instead of 500, masking defects.

### Exception Hierarchy

```
Exception
├── System.ArgumentException   ← thrown by Guard.Against.* for defensive checks
└── DomainValidationException  ← thrown by domain entities for invariant violations
      Path: Core/Exceptions/DomainValidationException.cs
```

`DomainValidationException` carries:
- `Message`: human-readable description of the violation
- `ParameterName`: optional name of the failing parameter

### Error Handling Flow

```
Client Request
  │
  ▼
Middleware Pipeline (CORS, Auth, Rate Limiting)
  │
  ▼
FastEndpoints Validator (FluentValidation) ──────────────► 400 (malformed input)
  │
  ▼
Mediator Pipeline
  ├── DomainExceptionBehavior ◄── catches DomainValidationException → Result.Error()
  ├── LoggingBehavior
  └── CurrentUserBehavior
  │
  ▼
UseCase Handler → calls domain factory/method
  │
  ├── Factory throws DomainValidationException → DomainExceptionBehavior → 400
  └── Successful Result<T> → Endpoint maps to 2xx
  │
  ▼
GlobalExceptionHandler ◄── catches everything else → 500 (ProblemDetails)
```

### Layer 1: Mediator Pipeline — Domain Exceptions → 400

**Component:** `DomainExceptionBehavior<TRequest, TResponse>` at `Web/Configurations/DomainExceptionBehavior.cs`

- Registered as the first `IPipelineBehavior<,>` in MediatorConfig
- Wraps every handler invocation in a try-catch for `DomainValidationException`
- Converts caught exceptions to `Result<T>.Error()` or `Result.Error()` via reflection
- Non-domain exceptions propagate through — not caught here

### Layer 2: ASP.NET Core Global Handler — Everything Else → 500

**Component:** `GlobalExceptionHandler` at `Web/Configurations/GlobalExceptionHandler.cs`

- Implements `IExceptionHandler`
- Registered via `services.AddExceptionHandler<T>()` and `app.UseExceptionHandler()`
- Returns RFC 7807 Problem Details JSON (`application/problem+json`)
- Logs full stack trace at Error level
- Never leaks stack traces or internal details to the client

### Where Each Tool Is Used

**`DomainValidationException`** (domain entities and value objects):
```csharp
// Core/RouteAggregate/Route.cs
public static Route Create(string name, long createdById)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new DomainValidationException("Route name is required.", nameof(name));
    if (createdById <= 0)
        throw new DomainValidationException("CreatedById must be positive.", nameof(createdById));
    return new Route(new RouteName(name), new UserId(createdById));
}
```

**`Guard.Against.*`** (infrastructure, configuration, pipeline behaviors):
```csharp
// Infrastructure — config validation (startup crash if missing)
Guard.Against.Null(connectionString);

// Pipeline behavior — defensive null check on internal framework parameter
Guard.Against.Null(request);
```

### Files

| File | Layer | Purpose |
|------|-------|---------|
| `Core/Exceptions/DomainValidationException.cs` | Core | Domain exception class |
| `Web/Configurations/DomainExceptionBehavior.cs` | Web | Mediator pipeline: DomainValidationException → Result.Error() |
| `Web/Configurations/GlobalExceptionHandler.cs` | Web | ASP.NET Core: unhandled exceptions → ProblemDetails 500 |
| `Web/Configurations/MediatorConfig.cs` | Web | Registers DomainExceptionBehavior as first pipeline behavior |
| `Web/Configurations/ServiceConfigs.cs` | Web | Registers GlobalExceptionHandler and ProblemDetails |
| `Web/Configurations/MiddlewareConfig.cs` | Web | Adds UseExceptionHandler() to middleware pipeline |

## Domain Invariants
- `DomainValidationException` must only be thrown from domain entities and value objects (Core layer)
- `DomainExceptionBehavior` catches only `DomainValidationException`, never `ArgumentException` or `Exception`
- `GlobalExceptionHandler` catches everything else — produces ProblemDetails with no stack traces
- `Guard.Against.*` is reserved for defensive programming in non-domain code
- Handlers must NOT contain try-catch for domain exceptions (the pipeline handles it)

## Event Impact
- None — this is infrastructure/plumbing, no domain events published or consumed.

## Acceptance Criteria
1. Domain factory method throws `DomainValidationException` → `DomainExceptionBehavior` catches → endpoint returns 400 with error message
2. Domain factory method throws `DomainValidationException` → handler code is clean (no try-catch) → pipeline handles it
3. Unhandled `NullReferenceException` or `InvalidOperationException` → `GlobalExceptionHandler` → 500 ProblemDetails response
4. `Guard.Against.Null(connectionString)` in Infrastructure → throws at startup (crash), never reaches GlobalExceptionHandler
5. `Result<T>` returned from handler without exception → endpoint maps via `ResultExtensions` to correct HTTP status

## Rollout Notes
- No database migration required
- No breaking API contract changes
- `Ardalis.GuardClauses` package must remain in Core for defensive checks
- All domain entities must use `DomainValidationException`, not `Guard.Against.*`, for invariant enforcement
