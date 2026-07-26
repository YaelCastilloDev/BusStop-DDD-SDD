# SPEC-Architecture-ResiliencePatterns

## Spec ID
`SPEC-Architecture-ResiliencePatterns`

## Bounded Context Owner
Cross-cutting / Architecture — resilience is infrastructure-wide, not owned by a single bounded context.

## Problem Statement
The application had only one resilience layer: `AddStandardResilienceHandler()` applied globally to all `HttpClient` instances. Three blind spots existed:

1. **Mediator pipeline** — no retry, timeout, or circuit breaker for handler execution. Transient DB/RabbitMQ errors propagated directly as 500.
2. **MassTransit** — no `UseMessageRetry`, `UseCircuitBreaker`, or delayed redelivery. Transient failures dropped messages permanently.
3. **EF Core** — no `EnableRetryOnFailure()`. Transient Postgres errors failed immediately.

## Domain Invariants
- **Idempotent requests** (queries/reads) are safe to retry.
- **Non-idempotent requests** (commands/writes) MUST NOT be retried automatically.
- `Result<T>` errors are business failures — they must never trigger retry.
- Only infrastructure exceptions trigger resilience strategies.
- Resilience pipelines are configured once in ServiceDefaults.

## Use-Case Slice Path
No new use cases. Architectural enhancement across:
- `Core/Interfaces/IIdempotentRequest.cs`
- `ServiceDefaults/Extensions.cs`
- `Web/Configurations/ResilienceBehavior.cs`
- `Infrastructure/InfrastructureServiceExtensions.cs`
- `Infrastructure/Integrations/RabbitMQ/RabbitMqRegistration.cs`

## Layer File Checklist
- **Core**: `Interfaces/IIdempotentRequest.cs` — marker interface
- **UseCases**: 9 queries → implement `IIdempotentRequest`
- **Infrastructure**: EF Core `EnableRetryOnFailure` + MassTransit retry/circuit-breaker/delayed-redelivery
- **Web**: `Configurations/ResilienceBehavior.cs` + pipeline registration
- **ServiceDefaults**: `AddResiliencePipelines()` + Polly package

## Command/Query and Endpoint Impact
No new endpoints. All existing endpoints benefit from the new pipeline behavior transparently.

## Event Impact
- **Published:** None
- **Consumed:** None

## Acceptance Criteria
1. Transient `NpgsqlException` during a `GetById` → ResilienceBehavior retries 3x with exp backoff; succeeds on recovery.
2. `CreateRoute` command transient exception → NO retry (non-idempotent); exception propagates.
3. MassTransit consumer transient failure → retries 5x with exp backoff before error queue.
4. MassTransit consumer trips circuit breaker (15 failures) → circuit opens; break duration active.
5. Poison message → delayed-redelivered at 1min, 5min, 15min; dead-lettered after.
6. EF Core transient Postgres error → `EnableRetryOnFailure` retries 2x, max 15s total.
7. Architecture test: all `IQuery<>` types implement `IIdempotentRequest`.
8. Architecture test: no `ICommand<>` types implement `IIdempotentRequest`.
9. Architecture test: `ResilienceBehavior` is in Web assembly and references Polly.
10. Unit test: `ResilienceBehavior` selects correct pipeline key per request type.

## Test Strategy
| Test Project | Type | Covers |
|---|---|---|
| **UnitTests** | Architecture (NetArchTest) | AC7, AC8, AC9: Layer boundaries, marker interface convention, behavior location |
| **UnitTests** | Behavior unit tests | AC10: ResilienceBehavior pipeline key selection, delegation, cancellation |
| **IntegrationTests** | EF Core retry | AC6: Transient Postgres failover via Testcontainers |
| **IntegrationTests** | MassTransit resilience | AC3, AC4, AC5: Consumer retry + circuit breaker via Testcontainers RabbitMQ |

## Rollout / Rollback Considerations
- **No migration needed** — all changes are code-only.
- **EF Core retry** applied globally; future iterations may tune per operation type.
- **MassTransit circuit breaker** trips at 15 failures — monitor RabbitMQ dashboard.
- **Rollback**: Remove `ResilienceBehavior` from `MediatorConfig` pipeline, revert `EnableRetryOnFailure`, comment out MassTransit retry config.
