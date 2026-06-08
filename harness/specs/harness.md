# Harness Spec

## Purpose
Define how the project harness governs spec-driven delivery for BusStop.
The harness must reduce ambiguity, preserve architecture boundaries, and accelerate safe iteration.

## Scope
- Spec lifecycle management.
- Validation gates before implementation and merge.
- Rule enforcement for DDD vocabulary and boundaries.
- Agent-compatible structure for planning and execution.

## Non-Goals
- Replacing CI/CD pipelines.
- Generating full production code without human review.
- Managing runtime infrastructure directly.

## Inputs
- Product intent from `harness/product/vision.md`.
- System constraints from `harness/system-design.md`.
- Structural rules from `harness/specs/clean-architecture-conventions.md`.
- Feature spec authored for one bounded context.
- Current repository state and project rules.

## Outputs
- Actionable implementation checklist.
- Traceability links from spec to code changes and tests.
- Pass/fail status for each validation gate.
- Decision log for deferred or rejected changes.

## Runtime Modes
- Plan mode: analyze, propose, and scope without modifying code.
- Agent mode: implement approved tasks with guardrails.
- Review mode: verify consistency, tests, and contract alignment.

## Harness Lifecycle
1. Intake: read feature request and map to bounded context.
2. Spec draft: define problem, invariants, acceptance criteria.
3. Validation preflight: check glossary, ownership, and dependencies.
4. Implementation: execute smallest vertical slice first.
5. Verification: run tests/lints and contract checks.
6. Closure: update docs/changelog and record outcomes.

## Required Feature Spec Template
- Title and bounded context owner.
- Problem statement and user impact.
- Domain invariants (must stay true).
- Use-case slice path (e.g., `UseCases/Routes/Create/`).
- Layer file checklist (Core, UseCases, Infrastructure, Web, tests; AspireHost/ServiceDefaults optional).
- Command/query and endpoint impact.
- Event impact (published/consumed events).
- Acceptance criteria (testable statements).
- Rollout and rollback considerations.

## Validation Gates
- Gate 1: Context Ownership
  - Feature belongs to exactly one primary bounded context.
- Gate 2: Domain Language
  - Terms match shared glossary (Route, Stop, Contribution, ModerationAction).
- Gate 3: Clean Architecture Compliance
  - Code placed in correct Ardalis layer (Core, UseCases, Infrastructure, Web).
  - Dependencies point inward; Core has no framework references.
  - Aggregate-folder structure (`{Entity}Aggregate/`) and naming conventions followed.
- Gate 4: Pattern Compliance
  - Guard clauses for domain invariants.
  - Specifications for non-trivial queries (no inline repository LINQ).
  - Result pattern for expected handler failures.
  - FastEndpoints validators present for all input-bearing endpoints (Web layer).
- Gate 5: Contract Safety
  - API/event changes are versioned and documented.
- Gate 6: Test Evidence
  - Unit, integration, and functional tests map to acceptance criteria.

## Guardrails
- No merge without an approved feature spec.
- No direct cross-context data writes.
- No hidden side effects outside declared events.
- No undocumented contract-breaking changes.

## Traceability Rules
- Every feature gets a stable spec ID.
- Commits and PRs reference spec ID.
- Tests include spec ID in naming or metadata.
- Decision log records accepted trade-offs and debts.

## Failure Handling
- If Gate 1-3 fails, stop implementation and return to planning.
- If Gate 4 fails, rework in the responsible layer agent before continuing.
- If Gate 5 fails, add version strategy before merge.
- If Gate 6 fails, reject merge and request missing evidence.

## Metrics
- Spec-to-merge lead time.
- Rework rate after review.
- Escaped defect rate by context.
- Acceptance criteria coverage ratio.
- Documentation drift incidents.

## Versioning and Evolution
- This harness spec is versioned with semantic intent.
- Major changes require migration notes for contributors and agents.
- Keep the harness minimal and evidence-driven.

## Canonical Spec Location
All harness specs live under `harness/specs/`. Cursor rules in `.cursor/rules/` provide runtime access for agents.
