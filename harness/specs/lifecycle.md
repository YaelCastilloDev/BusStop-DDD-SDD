# Feature Lifecycle Spec

## Purpose
Define the full path from idea to merge for harness-first, DDD-aligned delivery on BusStop.

## Lifecycle Stages
Full stage exit criteria and governance: `harness/specs/gates-guardrails.md`.

1. **Intake** — read feature request, map to bounded context, identify context owner.
2. **Spec Draft** — author a feature spec following the template below.
3. **Spec Approval** — validate spec against gates. If Gate 1-3 fails, stop and return to planning.
4. **Implementation** — execute the smallest vertical slice across all layers. Confirm layer placement against `harness/specs/clean-architecture-conventions.md`. For frontend, confirm style and component usage against `harness/specs/frontend-design-system.md`. Keep changes inside declared bounded context. Emit only declared events and contracts.
5. **Verification** — run tests/lints, check contracts, map evidence to acceptance criteria. Build and test: `dotnet build BusStop.slnx` && `dotnet test BusStop.slnx`.
6. **Review & Merge** — reviewer checks regression risks, boundary violations, missing tests, contract drift. No critical findings unresolved. Validation gates passed.
7. **Post-Merge Closure** — link merge artifacts to spec ID. Capture follow-up tasks for deferred work. Update backlog with residual risks.

## Failure Loop
- Unclear spec → return to Stage 2.
- Architecture conflict → return to Stage 3.
- Test failures → remain in Stage 5.
- Boundary issues → return to Stage 4.

## Feature Spec Template
Every feature must have a spec (`SPEC-<context>-<name>`) covering:
1. **Title + bounded context owner.**
2. **Problem statement** and user impact.
3. **Domain invariants** (must stay true).
4. **Use-case slice path** (e.g., `UseCases/Routes/Create/`).
5. **Layer file checklist:** Core, UseCases, Infrastructure, Web, tests (AspireHost/ServiceDefaults optional).
6. **Command/query and endpoint impact.**
7. **Event impact** (published/consumed events).
8. **Acceptance criteria** (testable, Given/When/Then style).
9. **Rollout and rollback** considerations.

Acceptance criteria format: Given context, when action, then expected observable result.
Decision log format: Decision, rationale, impact, revisit trigger.

## Cadence
- Slices small enough for single-PR review.
- Frequent merges over long-lived branches.
- Re-plan immediately when assumptions change.

## Context Tiers
Three-tier context model for agent decision-making. See `harness/specs/context.md` for the full Context Agent specification.

| Tier | Scope | Refresh Rule |
|---|---|---|
| **Tier 1 — Global** | Product vision, architecture, conventions, agent roles, rules, glossary | Revalidate when terminology or boundaries challenged |
| **Tier 2 — Feature** | Active spec, impacted contexts, contracts, open risks | Refresh at each stage handoff |
| **Tier 3 — Task** | Current files, diffs, test results, blockers | Refresh after every substantive code/test change |

## Governance
- No implementation before approved spec.
- No undocumented breaking contracts.
- No bypass of required verification evidence.
- Use canonical domain terms: Route, Stop, ModerationAction.
