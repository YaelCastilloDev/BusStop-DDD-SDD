# BusStop - System Design (Harness-First Baseline)

## Purpose
Define the minimum architecture required to deliver BusStop through spec-driven, DDD-aligned slices.
This document is the implementation baseline, not an exhaustive future-state blueprint.

## System Scope
- Contribution submission and moderation workflows.
- Search and read APIs for published transportation data.
- Auditability and operational visibility for all critical flows.

## Non-Goals (Current Phase)
- Offline-first synchronization.
- Android and iOS support.

## Architectural Style
- Clean Architecture ([ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture) template).
- Layer and naming rules: `harness/specs/clean-architecture-conventions.md`.
- Modular monolith initially, service extraction later when justified by load or team boundaries.
- Bounded contexts start as folders/modules inside `BusStop.*` projects, not separate services.
- Event-driven internal integration for async workflows.
- API-first contracts with explicit versioning.

## Bounded Contexts
- TransitCatalog: route, stop lifecycle and validation.
- ContributionModeration: review, escalation, undo, and rollback of changes already made within permissions.
- IdentityAccess: authentication, role claims, policy enforcement.
- SearchReadModel: optimized querying and indexing projections.
- AuditObservability: immutable action logs and operational signals.

## Context Map
- TransitCatalog publishes domain events consumed by SearchReadModel and AuditObservability.
- ContributionModeration orchestrates state transitions on TransitCatalog entities.
- IdentityAccess provides claims used by all write-side policies.
- AuditObservability is append-only and cannot mutate domain aggregates.

## High-Level Flow
1. Client sends authenticated command to API.
2. Application layer validates policy and domain invariants.
3. Aggregate mutation is persisted in the database.
4. Domain event is dispatched in-process (Phase 1) or published to message broker (Phase 2).
5. Workers update read/search projections and audit records.
6. Read endpoints serve from projection stores and replicas.

## Core Components

### Phase 1 (Current)
- API: ASP.NET Core with FastEndpoints REPR pattern, OpenAPI via Scalar.
- Auth: anonymous or simple policy-based authorization (expand in Phase 2).
- Storage: EF Core with SQLite (default), easily switched to SQL Server.
- Messaging: in-process domain event dispatch via Mediator.
- Orchestration: .NET Aspire host (optional).
- Observability: structured logs via Serilog, OpenTelemetry via ServiceDefaults.

### Phase 2 (Target)
- Edge: Cloudflare for TLS termination, caching, and edge protection.
- Auth: Keycloak (OIDC/OAuth2, JWT claims).
- Storage: PostgreSQL primary + read replicas (PostGIS candidate for geo queries).
- Messaging: RabbitMQ for integration and background processing.
- Outbox pattern for publish-after-commit consistency.

## Technology Baseline

### Phase 1 (Current)
- Backend: .NET 10 + ASP.NET Core.
- Architecture: Clean Architecture + CQRS use-case handlers via Mediator source generator.
- API: FastEndpoints (not Controllers).
- Data: EF Core with SQLite (default).
- Infra: Docker-first deployment, cloud-portable runtime.

### Phase 2 (Target)
- Data: PostgreSQL (PostGIS candidate when geo queries require it).
- Messaging: RabbitMQ with durable queues and retry policies.
- Auth: Keycloak centralized identity.

## Data Design Principles
- Route, Stop are aggregate roots with explicit invariants.
- Contribution is a first-class aggregate linked to target entity and decision state.
- ModerationAction is immutable and append-only for accountability.
- Use optimistic concurrency for conflicting edits.
- Use outbox pattern to guarantee publish-after-commit consistency (Phase 2).

## API and Contract Rules
- FastEndpoints are the default entry points for all domain operations.
- Commands and queries are separated at application layer.
- DTO contracts are stable; breaking changes require version bump.
- Sensitive writes require role policy and remain reviewable/reversible by higher roles.

## Event Model (Initial Set)
- RouteCreated
- RouteUpdated
- StopCreated
- ContributionSubmitted
- ContributionReviewed
- ContributionUndone
- ModerationActionRecorded

## Quality Attributes
- Scalability: horizontal API scale, async workers, read replicas (Phase 2).
- Reliability: retries, dead-letter queues (Phase 2).
- Security: centralized identity and policy-based authorization.
- Maintainability: explicit boundaries and spec-first slices.
- Observability: latency/error/queue/depth dashboards.

## Operational Baseline
- Health checks for API, DB connectivity, queue reachability.
- Structured logging with correlation IDs per request/command.
- SLO-oriented metrics: request latency, error rate, queue lag, review time.
- Alerting for moderation backlog growth and consumer failures.

## Risks and Mitigations
- Risk: domain drift across contexts.
  - Mitigation: shared glossary and invariant checks in specs.
- Risk: eventual consistency confusion for clients.
  - Mitigation: document read-after-write behavior in API contracts.
- Risk: moderation bottlenecks.
  - Mitigation: queue prioritization and workload metrics.

## Delivery Constraints
- Every feature starts with a short spec (problem, invariants, acceptance tests).
- No implementation without bounded-context ownership.
- No merge without test evidence and updated docs when contracts change.
- Harness rules enforce terminology and architectural boundaries.

## Evolution Path
- Start as modular monolith with strict module boundaries.
- Extract independent services only when operational data proves need.
- Add specialized search and geo infrastructure incrementally.
