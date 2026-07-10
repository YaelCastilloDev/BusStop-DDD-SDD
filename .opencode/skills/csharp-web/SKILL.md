---
name: csharp-web
description: BusStop.Web FastEndpoints and API patterns. Use when working in BusStop.Web project or implementing API endpoints.
---

# BusStop.Web Patterns

## Endpoints
- FastEndpoints REPR: one class per operation (`Create.cs`, `GetById.cs`, `List.cs`, `Update.cs`, `Delete.cs`).
- Inherit `Endpoint<TRequest, TResponse>` or typed `Results<...>`.
- Delegate all logic to Mediator commands/queries — endpoints are thin.

## DTOs and Validators
- Co-locate: `Create.CreateRequest.cs`, `Create.CreateValidator.cs`.
- Or nest in endpoint file for small features.
- Every input-bearing endpoint must have a `Validator<TRequest>` (FluentValidation via FastEndpoints).
- Validators are the first line of defense — catch malformed input before it reaches the handler or domain layer.

## Error Handling Context
Validators are Layer 1 of the two-tier error strategy (see `csharp-core` skill):
1. FastEndpoints Validator → 400 on malformed input
2. Domain factories return `Result<T>` (no exceptions) — handlers match on Result, endpoints map via `ResultExtensions`
3. `DomainExceptionBehavior` pipeline is kept as a safety net in `Configurations/` — catches any remaining `DomainValidationException` and converts to `Result.Error()`. No Core code should throw it.

## Result Mapping
- Use `ResultExtensions.ToCreatedResult()`, `.ToGetByIdResult()`, etc.
- Never throw for expected flow control — map `Result` status to HTTP.

## Configuration
- `Configurations/MediatorConfig.cs` — register Mediator source generator.
- `Configurations/ServiceConfigs.cs` — DI registration.
- `Configurations/MiddlewareConfig.cs` — pipeline setup.

## DI Pattern
```csharp
public class Create(IMediator mediator) : Endpoint<...>
{
  private readonly IMediator _mediator = mediator;
}
```
Always assign primary constructor parameters to private `_fields`. Never use constructor parameters directly in method bodies.

## API Conventions
- Route groups per bounded context: `/routes`, `/stops`, `/moderation`.
- OpenAPI tags per aggregate/feature.
- Authorization policies per role where required.
