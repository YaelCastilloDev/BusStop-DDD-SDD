# Agents Spec

## Purpose
Define agent roles, handoffs, and governance for harness-first development on BusStop.
All implementation agents must follow `harness/specs/clean-architecture-conventions.md` and the [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture) template.

## Operating Principles
- Spec-first: no implementation without a validated spec.
- Template-faithful: layer placement, naming, and patterns match Ardalis conventions.
- Boundary-safe: respect bounded contexts and the dependency rule.
- Evidence-based: claims map to tests, logs, or diffs.
- Human-controlled: maintainers approve merge.

## Architecture Reference
Before any code work, agents load:
1. `harness/specs/clean-architecture-conventions.md`
2. `harness/system-design.md` (bounded contexts)
3. Active feature spec

## Agent Catalog

### 1) Planner Agent
**Responsibilities:**
- Convert requests into concise feature specs.
- Map work to bounded context and Core/UseCases feature slice.
- Define domain invariants, acceptance criteria, and test matrix.
- Identify affected layers: Core aggregate, UseCase handler, Infrastructure spec, Web endpoint.

**Deliverables:**
- Spec with slice path (e.g., `UseCases/Routes/Create/`).
- File checklist: aggregate, command, handler, specification, endpoint with validator, tests.
- Event impact list (published/consumed domain events).

**Must specify per slice:**
- Aggregate root and invariants.
- Command vs query classification.
- Permission model (who can act; who can review/undo).
- Soft-delete or undo behavior when applicable.

---

### 2) Domain Agent
**Responsibilities:**
- Design and implement Core layer artifacts only.
- Define aggregates, value objects, domain events, and domain exceptions.
- Enforce invariants with Guard clauses; no infrastructure references.

**Naming rules:**
- Aggregates: `{Entity}Aggregate/` folder with singular entity file (e.g., `RouteAggregate/Route.cs`).
- Value objects: co-located in aggregate folder (e.g., `RouteName.cs`).
- IDs: Vogen `[ValueObject<T>]` structs (e.g., `RouteId`).
- Events: past tense + `Event` suffix (`RouteSoftDeletedEvent`).
- Errors: static `Error` records in `<Entity>Errors.cs`.

**Deliverables:**
- Core classes with invariants documented in code.
- Domain event definitions for state transitions worth auditing.
- Specifications in `{Aggregate}/Specifications/`.

**Forbidden:**
- EF attributes, DbContext, HTTP, ASP.NET Core in Core.
- Use-case orchestration handlers in Core (domain event handlers using `INotificationHandler<T>` are allowed).

> **Reference:** The template ships with `ContributorAggregate` as a reference vertical slice. Follow its patterns until BusStop aggregates replace it.

---

### 3) UseCase Agent
**Responsibilities:**
- Implement Application layer slices: commands, queries, handlers, DTOs.
- Organize by feature folder (`Routes/Create/`, not `Commands/`).
- Return `Result` / `Result<T>` for expected failures.

**Naming rules:**
- `CreateRouteCommand`, `CreateRouteHandler`.
- `GetRouteByIdQuery`, `GetRouteByIdHandler`.
- Response DTOs: `RouteResponse`, `RouteListItemResponse`.

**Deliverables:**
- Complete vertical use-case slice ready for Infrastructure and Web wiring.
- Query service interfaces for read-optimized queries when needed.

**Patterns:**
- Handlers implement `ICommandHandler<,>` from Mediator (not MediatR).
- Handlers depend on repository/specification interfaces defined in Core.
- No direct DbContext access — use abstractions.
- Map entities to DTOs inside handlers, not in endpoints.
- Validators are owned by the Web Agent (FastEndpoints), not UseCases.

---

### 4) Infrastructure Agent
**Responsibilities:**
- Implement EF Core configurations, repositories, specifications usage, and integrations.
- Wire domain event dispatch via `MediatorDomainEventDispatcher`.

**Naming rules:**
- `AppDbContext` in `Data/`.
- `RouteConfiguration` in `Data/Config/`.
- Specifications are authored in Core (`RouteByIdSpec`); Infrastructure uses them via `EfRepository<T>`.

**Deliverables:**
- EF configuration matching domain model.
- Query service implementations in `Data/Queries/`.
- Repository methods that accept specifications, not raw LINQ expressions.

**Forbidden:**
- Business rules or permission checks in Infrastructure.

---

### 5) Web Agent
**Responsibilities:**
- Implement API endpoints using FastEndpoints REPR pattern.
- Map HTTP requests to commands/queries; map `Result` to HTTP responses via `ResultExtensions`.
- Create FastEndpoints validators for all input-bearing endpoints.

**Naming rules:**
- Endpoint classes: `Create.cs`, `GetById.cs`, `List.cs`, `Delete.cs`.
- Co-located request/response: `CreateRouteRequest`, `CreateRouteResponse`.
- Co-located validators: `Create.CreateValidator.cs` or nested in endpoint file.
- Route groups aligned to bounded context (`/routes`, `/stops`, `/moderation`).

**Deliverables:**
- Thin endpoints that delegate all logic to UseCase handlers.
- OpenAPI tags and authorization policies applied per role.
- DI configuration in `Configurations/` (MediatorConfig, ServiceConfigs, etc.).

**Forbidden:**
- Business logic, validation rules, or direct DbContext in endpoints.
- Using primary constructor parameters directly — assign to `_privateFields`.

---

### 6) Reviewer Agent
**Responsibilities:**
- Verify acceptance criteria, layer boundaries, and Ardalis naming compliance.
- Check dependency direction (Core has no outward refs).
- Flag missing validators, specifications, tests, or undocumented contract changes.

**Review checklist:**
- [ ] Files in correct project/layer.
- [ ] Aggregate-folder structure in Core (`{Entity}Aggregate/`).
- [ ] Feature-slice folder structure in UseCases.
- [ ] Guard clauses on domain invariants.
- [ ] Specifications used instead of inline LINQ.
- [ ] Result pattern for expected failures.
- [ ] FastEndpoints validators on input-bearing endpoints.
- [ ] Endpoints are thin; no business logic in Web.
- [ ] Permission + review/undo flow matches product spec.

**Deliverables:**
- Severity-ordered findings.
- Merge recommendation: approve / rework / block.

---

### 7) Test Agent
**Responsibilities:**
- Write and run tests matching the three-project test strategy.
- Map every acceptance criterion to at least one test.

**Test placement:**
- **UnitTests:** entity invariants, value objects, specification logic, use case handlers.
- **IntegrationTests:** EF config, repository + specification against real/test DB.
- **FunctionalTests:** HTTP routes via `WebApplicationFactory`.

**Deliverables:**
- Pass/fail evidence per acceptance criterion.
- Coverage gaps and remaining risks.

---

### 8) Context Agent
**Responsibilities:**
- Assemble context packs from Tier 1 docs + active spec + current diff.
- Detect glossary drift and stale architecture assumptions.
- Ensure agents loaded `clean-architecture-conventions.md` before coding.

**Deliverables:**
- Context pack with source references.
- Drift warnings and reconciliation actions.

## Handoff Protocol
1. Planner -> Domain/UseCase/Infrastructure/Web agents (approved spec + file checklist).
2. Implementation agents -> Reviewer (diff summary + contracts changed).
3. Reviewer -> Test Agent (verification checklist).
4. Test Agent -> Maintainer (evidence + risks).

## Governance Rules
- Agents cannot bypass harness validation gates.
- Agents cannot redefine domain terms (Route, Stop, Contribution, ModerationAction).
- Agents cannot place code in wrong layers even if faster.
- Agents must escalate ambiguity instead of guessing.

## Failure Escalation
- Missing domain decision -> Planner.
- Layer or naming violation -> rework in responsible agent.
- Failing critical tests -> block merge.
- Context drift -> Context Agent refresh before continuing.

## Security
- Never expose secrets in outputs.
- Redact configuration values.
- Treat external input as untrusted until validated.
