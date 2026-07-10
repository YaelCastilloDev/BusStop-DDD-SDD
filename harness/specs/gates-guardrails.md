# Validation Gates & Guardrails

## Purpose
Define the validation gates that must pass before merge, and the non-negotiable guardrails for BusStop development.

## Validation Gates
All six must pass before merge.

### Gate 1 — Context Ownership
Feature belongs to exactly one primary bounded context. Bounded contexts: TransitCatalog, IdentityAccess, SearchReadModel, AuditObservability.
**Owner: Planner + Context agents verify.**
**Fail → Stop. Return to planning.**

### Gate 2 — Domain Language
Terms match shared glossary: Route, Stop, ModerationAction. No unauthorized synonyms.
**Owner: Context agent detects drift; busstop-domain skill defines canon.**
**Fail → Stop. Return to planning.**

### Gate 3 — Clean Architecture Compliance
Code placed in correct Ardalis layer. Dependencies point inward. Core has no framework references. Aggregate-folder structure (`{Entity}Aggregate/`) and naming conventions followed. See `harness/specs/clean-architecture-conventions.md`.
**Owner: Domain + UseCase + Infrastructure + Web agents comply; clean-architecture skill defines rules; Reviewer verifies.**
**Fail → Stop. Return to planning.**

### Gate 4 — Pattern Compliance
Two-tier error strategy: Result pattern for expected failures (factory methods), Guard clauses for impossible failures (internal constructors). Specifications for non-trivial queries (no inline repository LINQ). FastEndpoints validators present for all input-bearing endpoints (Web layer).
**Owner: Domain + UseCase + Web agents enforce; csharp-core, clean-architecture, csharp-web skills define. Reviewer verifies.**
**Fail → Rework in responsible layer agent before continuing.**

### Gate 5 — Contract Safety
API/event changes are versioned and documented.
**Owner: Web + Infrastructure agents responsible; Reviewer verifies.**
**Fail → Add version strategy before merge.**

### Gate 6 — Test Evidence
Unit, integration, and functional tests map to every acceptance criterion.
**Owner: Test agent provides traceability; Reviewer confirms.** 
**Fail → Reject merge. Request missing evidence.**

## Failure Handling
- **Gate 1-3 failure:** Stop implementation, return to planning.
- **Gate 4 failure:** Rework in the responsible layer agent before continuing.
- **Gate 5 failure:** Add version strategy before merge.
- **Gate 6 failure:** Reject merge and request missing evidence.

## Traceability Rules
- Every feature gets a stable spec ID (`SPEC-<context>-<name>`).
- Commits and PRs reference spec ID.
- Tests include spec ID in naming or metadata.
- Decision log records accepted trade-offs and debts.

## Guardrails
- No merge without an approved feature spec.
- No direct cross-context data writes.
- No hidden side effects outside declared events.
- No undocumented contract-breaking changes.
- Every use case gets: command/query + handler + endpoint + tests.

## Drift Prevention
- Use canonical terms only: Route, Stop, ModerationAction.
- Reject synonyms that alter domain meaning unless glossary is updated.
- Flag contradictions between specs and code as blockers.
- Require explicit migration notes when contracts change.
- Discard stale assumptions not backed by current sources.
- Revalidate global context when terminology or boundaries are challenged.
