# RabbitMQ Outbox Audit

**Audit date:** 2026-09-03  
**Scope:** Current working tree, repository specifications, RabbitMQ deployment configuration, MassTransit configuration, EF Core persistence, integration events, and messaging tests.

## Executive verdict

The current implementation is **not a transactional outbox implementation and is not production-ready for publish-after-commit reliability**.

RabbitMQ and MassTransit are wired successfully for direct publishing and basic consumption, but the application does not persist outgoing messages with the business transaction. Domain events are dispatched after `SaveChangesAsync` has completed, and the integration handlers call `IPublishEndpoint.Publish` directly. A process crash or broker failure in that interval can permanently lose an event, while a publish failure can be reported to the API after the database transaction has already committed.

The repository’s own system design identifies the outbox as the Phase 2 mechanism for publish-after-commit consistency. The current code has reached the RabbitMQ transport stage, but not the outbox stage.

## Evidence inspected

| Area | Evidence | Assessment |
|---|---|---|
| Broker registration | `src/BusStop.Infrastructure/Integrations/RabbitMQ/RabbitMqRegistration.cs` | MassTransit RabbitMQ transport is registered and endpoint discovery is enabled. |
| Publishing | `ModerationActionRecordedEventPublisher` and `UserRegisteredIntegrationHandler` | Both publish directly through `IPublishEndpoint`. |
| Database event timing | `src/BusStop.Infrastructure/Data/EventDispatcherInterceptor.cs` | Events are dispatched from `SavedChangesAsync`, after EF Core reports a successful save. |
| Outbox package | `src/BusStop.Infrastructure/BusStop.Infrastructure.csproj` and `Directory.Packages.props` | `MassTransit.EntityFrameworkCore` is absent. |
| Outbox schema | `src/BusStop.Infrastructure/Migrations/` and `AppDbContext` | No `InboxState`, `OutboxMessage`, or `OutboxState` entities or tables exist. |
| Delivery service | MassTransit registration | No bus-outbox delivery service is configured. |
| Consumer inbox | Receive endpoint configuration | No transactional consumer outbox or duplicate detection is configured. |
| Tests | `MassTransitIntegrationTests` | Tests cover startup and basic round-trip publishing only. |
| Build/test baseline | Verification run during this audit | Solution build passed; 286 unit tests and 3 RabbitMQ smoke tests passed. |

## Current message flow

The effective flow for an integration event is:

```text
Application command
    -> EF Core SaveChangesAsync
    -> database commit succeeds
    -> EventDispatcherInterceptor.SavedChangesAsync
    -> Mediator domain-event handlers
    -> IPublishEndpoint.Publish
    -> RabbitMQ
```

The database commit and broker publish are separate operations. This is a dual-write pattern, not an outbox pattern.

MassTransit’s transactional outbox instead stores the outgoing message in the same database transaction and uses a delivery service to publish it after the transaction commits. Its consumer outbox additionally records received message identity for duplicate detection.

## Findings

### P0 — No durable transactional outbox exists

**Evidence:** There is no `MassTransit.EntityFrameworkCore` reference, no `AddEntityFrameworkOutbox`, no `UseBusOutbox`, no outbox model configuration, and no outbox migration.

**Risk:** A database update can commit while its RabbitMQ event is lost. The reverse ordering is also unsafe: publishing before a database transaction commits could expose state that later rolls back. The current post-save ordering avoids the second case but does not solve the first.

**Required direction:** Add MassTransit’s EF Core transactional bus outbox to `AppDbContext`, using PostgreSQL locking and the same scoped `DbContext` as application writes.

### P0 — Post-commit publishing creates an unrecoverable failure window

**Evidence:** `EventDispatcherInterceptor.SavedChangesAsync` calls `DispatchAndClearEvents` after EF Core save completion, and the integration handlers call `IPublishEndpoint.Publish` directly.

**Failure scenario:**

1. The route, user, or moderation action is committed.
2. The process crashes before the handler publishes the integration event.
3. No durable record identifies the missing event.
4. The downstream consumer never receives it.

A broker outage has a similar result if the application does not retain the message for later delivery. If publishing throws, the API may return an error even though its database operation already succeeded, causing clients or operators to retry a command and potentially create duplicate business effects.

### P0 — `ModerationActionId` can be published as zero

**Evidence:** `ModerationActionConfiguration` and the migration define `ModerationAction.Id` as a PostgreSQL identity column. `ModerationAction.Create` registers `ModerationActionRecordedEvent` with `action.Id` before the entity is inserted.

At factory time, the identity value is still the CLR default (`0`). EF Core receives the generated value during insert, but the already-created domain event retains `0`. The integration publisher copies that stale value into `ModerationActionRecordedIntegrationEvent`.

This affects both route and comment moderation because both create a `ModerationAction` before persistence. Existing unit tests assert the pre-persistence value and therefore do not expose the production behavior.

**Required direction:** Use an application-assigned identifier before event creation, or redesign event creation so the persisted identifier is available before the integration contract is materialized. Add an integration test that persists a moderation action and asserts the published/staged identifier is positive and matches the database row.

### P1 — No consumer outbox/inbox or duplicate protection

**Evidence:** No receive endpoint calls `UseEntityFrameworkOutbox<AppDbContext>`, and no inbox entities exist.

MassTransit and RabbitMQ provide at-least-once delivery semantics. A consumer can receive the same message again after a timeout, process restart, negative acknowledgement, or broker redelivery. The current repository has no transactional inbox or documented application-level idempotency key for message consumers.

**Required direction:** Configure the transactional consumer outbox for every consumer that changes durable state and publishes/sends messages. Make all externally visible handlers idempotent even when inbox deduplication is enabled.

### P1 — Retry policy is incomplete and contradicts the resilience specification

**Evidence:** `RabbitMqRegistration` configures only delayed redelivery at 5 seconds, 30 seconds, and 2 minutes. It does not configure `UseMessageRetry` or a circuit breaker. `SPEC-Architecture-ResiliencePatterns.md` describes five transient retries, a circuit breaker, and delayed redelivery at 1, 5, and 15 minutes.

Retry, redelivery, and outbox solve different problems:

- Retry handles short-lived transient failures while retaining the delivery.
- Redelivery returns a message to the broker for a later attempt.
- The outbox prevents messages from failed processing attempts from being emitted.

The policy must be explicitly aligned with the intended production SLO and updated in the specification. Business-rule exceptions should not be retried; infrastructure/transient failures should be filtered and retried according to the policy.

### P1 — Integration contracts are not stable enough for external consumers

`UserRegisteredIntegrationEvent` has no version field or documented compatibility policy. `ModerationActionRecordedIntegrationEvent` has a numeric `Version`, but there is no contract ownership, schema policy, explicit message name, or evolution rule.

The contracts also live inside the Infrastructure project, which makes independent external consumption and compatibility testing less clear. MassTransit’s default type-based topology can make namespace/type renames into broker contract changes.

**Required direction:** Define stable integration contracts in a dedicated contracts assembly or explicitly governed contract namespace. Add an event identifier, occurrence timestamp, correlation metadata, explicit stable message names, and a compatibility/versioning policy. Treat removed or renamed fields as breaking changes requiring a new contract version.

### P1 — Local notification side effects are not coordinated with durable event processing

`ModerationActionRecordedEventConsumer` is an in-process Mediator notification handler, not a RabbitMQ consumer. It persists a notification, calls the Resend email integration, and sends a SignalR message directly.

This means:

- Notification persistence and email delivery are not one atomic operation.
- SignalR delivery is ephemeral and cannot be replayed from the current event path.
- A failure after the database save can surface after the originating business transaction has committed.
- There is no durable retry record for email or SignalR failure.

This may be acceptable for a deliberately in-process Phase 1 workflow, but it must not be described as RabbitMQ-backed reliable processing. If these side effects require recovery, they should be driven by durable messages and idempotent consumers/outbox-backed follow-up messages.

### P1 — RabbitMQ deployment defaults are development-oriented

The Docker Compose configuration uses:

- Floating `rabbitmq:3-management` image tagging.
- Default `guest` credentials in configuration and startup scripts.
- A single broker container.
- No RabbitMQ data volume.
- No RabbitMQ health check.
- No API `depends_on` relationship for RabbitMQ.
- No explicit quorum-queue topology.

MassTransit receive endpoints are durable by default, but durable queues on a single ephemeral broker do not provide production availability or disaster recovery. Production deployment should pin the broker version, manage credentials as secrets, persist broker data, use a managed/clustered RabbitMQ deployment where required, and explicitly choose quorum queues for critical durable workloads.

### P2 — Operational controls are missing

There is no documented or tested operational handling for:

- Outbox backlog size and oldest-message age.
- Delivery failure count and retry age.
- Stuck or poison outbox messages.
- MassTransit error queues and replay procedures.
- Inbox cleanup and duplicate-detection retention.
- Contract/schema incompatibility failures.
- Graceful shutdown and draining behavior.

The existing OpenTelemetry and dashboard infrastructure is a useful foundation, but outbox-specific metrics, logs, and alerts still need to be added.

### P2 — Existing RabbitMQ tests are smoke tests, not reliability tests

The three messaging integration tests verify bus startup, one publish/consume round trip, and five published messages. They use fixed `Task.Delay` calls and a static received-message list. They do not verify transactional behavior, retries, duplicate delivery, restart recovery, or outbox delivery.

These tests should remain as connectivity checks but must be supplemented with deterministic tests that await a specific condition and inspect both PostgreSQL and RabbitMQ state.

## What is working

The following parts are valid foundations:

- MassTransit and RabbitMQ packages are registered and the solution builds.
- Configuration supports an injected messaging connection string and a local fallback.
- Consumer assembly scanning and automatic endpoint configuration are present.
- PostgreSQL EF Core retry-on-failure is enabled for database connectivity.
- Delayed redelivery is present as a starting point for message recovery.
- Existing unit and RabbitMQ smoke tests pass.
- The repository system design explicitly calls for an outbox, so the target architecture is already recognized by the project.

These strengths establish transport connectivity, not atomic publish-after-commit guarantees.

## Recommended target design

### 1. Add the MassTransit EF Core provider

Add `MassTransit.EntityFrameworkCore` at the same pinned version as the existing MassTransit packages (`8.4.0`) through central package management. Keep the provider, transport, and core MassTransit package versions aligned.

Register the EF Core outbox against `AppDbContext` with PostgreSQL locking and enable the bus outbox. The scoped `IPublishEndpoint` used by application/domain-service code must resolve to the outbox-backed endpoint.

### 2. Add the transactional schema

Include MassTransit’s three EF model entities in `AppDbContext`:

- `InboxState` — received-message identity and duplicate detection.
- `OutboxMessage` — serialized outgoing messages.
- `OutboxState` — delivery coordination and ordering for bus-outbox messages.

Create and apply an EF migration. Do not hand-design replacement tables unless a specific operational requirement justifies a custom outbox.

### 3. Separate bus-outbox and consumer-outbox responsibilities

- Use the **bus outbox** for messages published from API requests, application services, and post-commit domain-service workflows.
- Use the **consumer outbox** on RabbitMQ receive endpoints where a consumer updates durable state and emits messages.
- Use both when a service both initiates work from HTTP and consumes broker messages.

The delivery service must run in every instance that is allowed to drain outbox messages. Its concurrency, batch size, timeout, and delivery retry settings should be configured explicitly and monitored.

### 4. Make event data valid at persistence boundaries

Fix `ModerationActionRecordedEvent` so its identifier is not captured before the identity is generated. Prefer application-assigned IDs for events that need to be known before persistence, or stage the integration event only after the entity identifier is available while keeping staging in the same transaction.

Add a stable event identifier and correlation metadata. Use UTC timestamps consistently; `DateTimeOffset` is preferred for cross-process contracts.

### 5. Govern the integration contract

Create a contract policy covering:

- Stable message/entity names.
- Contract ownership and location.
- Required metadata.
- Additive versus breaking field changes.
- Versioning and deprecation windows.
- Serialization compatibility tests.
- Consumer behavior for unknown fields and newer versions.

Do not use a renamed CLR type or namespace as an accidental broker contract migration.

### 6. Harden RabbitMQ operations

For production, document and implement:

- Pinned RabbitMQ image/version.
- Secret-managed credentials and a non-default user.
- Durable broker storage and backup/restore procedures.
- Health checks and startup readiness.
- Explicit endpoint names and topology.
- Quorum queues for critical durable queues when the deployment supports a replicated cluster.
- Error-queue retention, alerting, replay, and poison-message handling.
- Outbox delivery and backlog dashboards.

## Required test matrix

| Scenario | Expected result |
|---|---|
| Business write succeeds | Business row and outbox row commit atomically. |
| Business transaction rolls back | No outbox message remains deliverable. |
| Broker unavailable after commit | Outbox row remains and is delivered after broker recovery. |
| Process restarts with pending rows | A new instance drains pending messages. |
| Delivery attempt fails | Delivery retry occurs without losing the outbox row. |
| Message exceeds retry policy | It is moved to the configured error/dead-letter path and is observable. |
| Same message is delivered twice | Consumer state changes and outgoing effects occur once. |
| Multiple application instances drain | No duplicate delivery caused by outbox race conditions. |
| Outbox retention expires | Delivered messages/inbox records are cleaned according to policy. |
| Route moderation | Published/staged moderation action ID is positive and matches PostgreSQL. |
| Contract serialization | Current and supported previous versions deserialize as expected. |
| Existing regression suite | Build, unit, integration, and functional tests remain green. |

## Implementation order

1. Add the EF Core outbox package and configuration.
2. Add model entities and migration.
3. Enable the bus outbox and verify API/domain-service publishing is staged in PostgreSQL.
4. Add consumer outbox configuration when the first RabbitMQ consumer is introduced.
5. Fix generated identifiers and formalize integration contracts.
6. Add failure-injection and duplicate-delivery tests.
7. Add metrics, alerts, error-queue procedures, and deployment hardening.
8. Update `SPEC-Architecture-ResiliencePatterns.md` so retry/redelivery behavior matches the implemented policy.

## Source guidance

- [MassTransit Outbox concepts](https://masstransit.io/documentation/patterns/transactional-outbox) — distinction between bus outbox, consumer outbox, retry, redelivery, and idempotency.
- [MassTransit outbox configuration](https://masstransit.io/documentation/configuration/middleware/outbox) — EF Core package, PostgreSQL configuration, model entities, bus delivery service, and consumer configuration.
- [MassTransit RabbitMQ configuration](https://masstransit.io/documentation/transports/rabbitmq) — durable receive endpoints, quorum queues, topology, and endpoint settings.
- [RabbitMQ publisher confirms](https://www.rabbitmq.com/docs/confirms) — broker confirmation and persistence semantics.
- [RabbitMQ quorum queues](https://www.rabbitmq.com/docs/quorum-queues) — replicated durable queues and data-safety considerations.
- [BusStop resilience specification](../harness/specs/SPEC-Architecture-ResiliencePatterns.md) — repository acceptance criteria for retry, circuit breaking, and redelivery.
- [BusStop system design](../harness/system-design.md) — intended Phase 2 outbox and RabbitMQ architecture.

## Final assessment

The current implementation should be treated as **RabbitMQ transport integration with post-commit direct publishing**, not as an outbox. It is suitable for development smoke testing and low-value best-effort notifications, but it does not meet the repository’s stated publish-after-commit consistency goal or normal production reliability expectations.

The highest-priority work is to add the transactional EF Core bus outbox, fix the pre-insert moderation-action identifier bug, and add failure/restart/duplicate-delivery tests before relying on RabbitMQ events for business-critical integration.
