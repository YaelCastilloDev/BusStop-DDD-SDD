# Context Strategy Spec

## Purpose
Define how project context is collected, prioritized, and refreshed so agents and developers make consistent decisions.

## Goals
- Minimize context-switch cost during feature delivery.
- Prevent domain language drift across docs and code.
- Keep context payloads concise and task-relevant.
- Ensure decisions remain traceable and current.

## Context Model
Context is split into three tiers:

### Tier 1 - Global Context
Long-lived constraints that apply to all work:
- Product vision and non-goals.
- System architecture and bounded contexts.
- Clean Architecture conventions (`harness/specs/clean-architecture-conventions.md`).
- Agent roles and handoffs (`harness/specs/agents.md`).
- Cursor rules (`.cursor/rules/`) for runtime agent guidance.
- Project rules and coding standards.
- Shared glossary and invariants.
- Supplementary C# style details (`.github/copilot-instructions.md`).

### Tier 2 - Feature Context
Feature-scoped data that evolves during implementation:
- Approved feature spec and acceptance criteria.
- Impacted contexts, contracts, and events.
- Open risks and unresolved decisions.

### Tier 3 - Task Context
Short-lived execution details:
- Current file set and active diffs.
- Test results and lint findings.
- Immediate blockers and assumptions.

## Retrieval Order
1. Global context files (`harness/product/vision.md`, `harness/system-design.md`, `harness/specs/clean-architecture-conventions.md`, `.cursor/rules/`, harness specs).
2. Active feature spec and linked notes.
3. Current code/diff and verification output.
4. External references only when required by the task.

## Source of Truth Rules
- Product intent: `harness/product/vision.md`.
- Architecture baseline: `harness/system-design.md`.
- Layer structure and naming: `harness/specs/clean-architecture-conventions.md`.
- Agent behavior: `harness/specs/agents.md`.
- Workflow and governance: remaining `harness/specs/*.md`.
- Runtime agent rules: `.cursor/rules/`.
- Feature intent: the active feature spec.
- Runtime truth: code + tests.

## Freshness Rules
- Refresh Tier 2 at each stage handoff.
- Refresh Tier 3 after every substantive code or test change.
- Revalidate Tier 1 when terminology or boundaries are challenged.
- Discard stale assumptions not backed by current sources.

## Drift Prevention
- Use canonical terms only: Route, Stop, Contribution, ModerationAction.
- Reject synonyms that alter domain meaning unless glossary is updated.
- Flag contradictions between specs and code as blockers.
- Require explicit migration notes when contracts change.

## Context Pack Template
Each active task should maintain a compact context pack:
- Task ID and spec ID.
- Primary bounded context.
- Invariants in scope.
- Files/contracts likely to change.
- Acceptance criteria checklist.
- Current risks and unknowns.

## Context Budget
- Keep active context under a practical size by prioritizing relevance.
- Prefer links/references over copied long text.
- Summarize, do not duplicate, stable documents.
- Archive stale task notes after merge.

## Escalation Rules
- Missing spec ID -> stop and request planning.
- Conflicting invariants -> escalate to planner/maintainer.
- Ambiguous context ownership -> resolve before implementation.
- Outdated architecture assumptions -> update docs before merge.

## Validation Checklist
- Correct bounded context selected.
- Domain vocabulary is consistent.
- Acceptance criteria are still measurable.
- Test evidence aligns with latest spec version.
- No unresolved contradiction in source-of-truth docs.

## Metrics
- Context retrieval time per task.
- Drift incidents detected in review.
- Rework caused by stale assumptions.
- Spec-to-code alignment score.

## Evolution
- Review this strategy after every major milestone.
- Add automation only where manual context management repeatedly fails.
