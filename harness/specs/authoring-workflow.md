# Spec Authoring Workflow

## Purpose
Provide a repeatable path from idea to merge for harness-first, DDD-aligned delivery.

## Workflow Summary
1. Intake request.
2. Frame bounded context and domain impact.
3. Author and approve feature spec.
4. Implement smallest valuable slice.
5. Verify against acceptance criteria.
6. Review, merge, and record decisions.

## Stage 1 - Intake
Inputs:
- Feature request or bug report.
- Product and architecture baseline docs.

Actions:
- Clarify user impact and expected behavior.
- Assign a tentative bounded context owner.
- Record assumptions and open questions.

Exit Criteria:
- Problem is clear and scoped.
- Context owner is identified.

## Stage 2 - Spec Draft
Required sections:
- Problem statement.
- Domain invariants.
- Use-case slice path and layer file checklist (Core, UseCases, Infrastructure, Web, tests; AspireHost/ServiceDefaults optional).
- Acceptance criteria.
- API/event/schema impact.
- Test strategy (unit, integration, functional).
- Rollout and rollback notes.

Rules:
- Keep spec concise and testable.
- Use canonical domain vocabulary.
- Include only implementation-relevant details.

Exit Criteria:
- Spec is review-ready and internally consistent.

## Stage 3 - Spec Approval
Review checks:
- Context ownership is correct.
- Invariants are explicit and coherent.
- Contract changes are version-safe.
- Risks and dependencies are acknowledged.

Exit Criteria:
- Spec approved with unique spec ID.

## Stage 4 - Implementation
Actions:
- Implement the smallest vertical slice first across layers.
- Follow Ardalis feature-slice layout and naming from `harness/specs/clean-architecture-conventions.md`.
- Keep changes inside declared bounded context.
- Emit only declared events and contracts.
- Update docs if behavior or interfaces changed.

Exit Criteria:
- Code compiles and maps directly to approved criteria.

## Stage 5 - Verification
Minimum evidence:
- Unit tests for domain invariants.
- Integration tests for API/event behavior.
- Lint/static checks for touched files.
- Manual checks for critical user flow if needed.

Build and test commands:
```bash
dotnet build BusStop.slnx
dotnet test BusStop.slnx
```

Exit Criteria:
- Every acceptance criterion has pass/fail evidence.

## Stage 6 - Review and Merge
Reviewer focus:
- Regression risks and boundary violations.
- Missing tests or weak assertions.
- Contract drift across docs, code, and events.

Merge Preconditions:
- No critical findings unresolved.
- Validation gates passed.
- Decision log updated for notable trade-offs.

## Stage 7 - Post-Merge Closure
Actions:
- Link merge artifacts to spec ID.
- Capture follow-up tasks for deferred work.
- Update backlog with residual risks and observability needs.

## Failure Loop
- If spec is unclear -> return to Stage 2.
- If architecture conflict appears -> return to Stage 3.
- If tests fail -> remain in Stage 5 until resolved.
- If review finds boundary issues -> return to Stage 4.

## Governance Constraints
- No implementation before approved spec.
- No undocumented breaking contracts.
- No bypass of required verification evidence.

## Lightweight Templates
Feature spec title:
- `SPEC-<context>-<short-name>`

Acceptance criterion style:
- Given context, when action, then expected observable result.

Decision log style:
- Decision, rationale, impact, revisit trigger.

## Cadence
- Keep slices small enough for single-PR review.
- Prefer frequent merges over long-lived branches.
- Re-plan immediately when assumptions change.
