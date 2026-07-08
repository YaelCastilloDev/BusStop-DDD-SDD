---
name: busstop-harness
description: BusStop spec-first workflow and validation gates. Use when implementing features, reviewing PRs, or following the spec-driven delivery workflow.
---

# BusStop Harness

Follow spec-driven delivery. Full spec: `harness/specs/lifecycle.md`.

## Before Implementation
1. Read active feature spec with spec ID.
2. Map work to exactly one bounded context.
3. Use canonical domain terms: Route, Stop, Contribution, ModerationAction.

## Lifecycle
1. Intake → 2. Spec draft → 3. Validation preflight → 4. Implementation (smallest vertical slice) → 5. Verification → 6. Closure

## Agents

| Agent | Role | Spec |
|---|---|---|
| **Planner** | Converts requests into validated specs; maps work to bounded contexts and feature slices. Use FIRST. | `harness/specs/planner.md` |
| **Context** | Assembles context packs from docs, specs, and diffs. Detects glossary drift. | `harness/specs/context.md` |
| **Domain** | Implements Core layer: aggregates, value objects, domain events, specifications. | `harness/specs/domain.md` |
| **UseCase** | Implements Application layer: commands, queries, handlers, DTOs via Mediator. | `harness/specs/usecase.md` |
| **Infrastructure** | Implements EF Core config, repositories, queries, event dispatch. | `harness/specs/infrastructure.md` |
| **Web** | Implements FastEndpoints REPR endpoints, validators, DI config. | `harness/specs/web.md` |
| **Reviewer** | Verifies code against validation gates. Read-only — never modifies code. | `harness/specs/reviewer.md` |
| **Test** | Writes unit, integration, and functional tests; maps every criterion to a test. | `harness/specs/test.md` |

Implementation order: Planner → Context → Domain → UseCase → Infrastructure → Web → Reviewer → Test

## Validation Gates (must pass before merge)
- **Gate 1 (Context Ownership):** Single bounded context owner. Planner + Context verify.
- **Gate 2 (Domain Language):** Terms match glossary. Context detects drift.
- **Gate 3 (Clean Architecture):** Correct layer placement, dependency flow, aggregate-folder structure. Domain + UseCase + Infrastructure + Web comply; Reviewer verifies.
- **Gate 4 (Pattern Compliance):** Guard clauses, Specifications, Result pattern, FastEndpoints validators. Reviewer verifies.
- **Gate 5 (Contract Safety):** Versioned/documented API and event changes. Reviewer verifies.
- **Gate 6 (Test Evidence):** Tests map to every acceptance criterion. Test agent provides; Reviewer verifies.

## Guardrails
- No merge without approved feature spec.
- No cross-context data writes.
- No undocumented breaking changes.
- Commits/PRs reference spec ID.
- Every use case: command/query + handler + endpoint + tests.

## Key References
- Product: `harness/product/vision.md`
- Architecture: `harness/system-design.md`
- Conventions: `harness/specs/clean-architecture-conventions.md`
- Lifecycle: `harness/specs/lifecycle.md`
- Gates: `harness/specs/gates-guardrails.md`
