# Moderation Model Discrepancy

## Current behavior

`Comment` and `Route` both persist `ModeratedAt` and `ModeratedBy`. Their EF Core global query filters use those fields to exclude moderated rows from normal reads:

- comments: `ModeratedAt == null`
- routes: `DeletedAt == null && ModeratedAt == null`

Consequently, normal public reads do not need to join or look up the `moderation_actions` table to exclude temporarily hidden content.

The current moderation endpoints immediately set the target's moderation fields and also create a `ModerationAction` record. This makes the target fields the current visibility projection and `ModerationAction` the petition/audit record.

## Recommended model boundary

Keep the current visibility state on the target (`Comment` or `Route`) so public reads remain a single-table query with a local filter. Use `ModerationAction` to retain the petition rationale and decision history, not as the source queried for every public content load.

Under the intended workflow, moderation means **temporarily hidden while a petition is pending**, rather than permanently removed:

1. A moderator files a petition and the target becomes temporarily hidden through its moderation fields.
2. If approved, the target is soft-deleted by setting `DeletedAt` and `DeletedBy`.
3. The rejection behavior is still to be defined; if rejected content should reappear, the pending-hide fields must be cleared.

Moderator and review workflows must intentionally bypass the public query filters when listing or resolving pending items. Public reads should continue to use the filters unchanged.

## Existing inconsistency

The current implementation does not fully support the intended approval step:

- `Route` has `DeletedAt` and `DeletedBy`, but `DELETE /routes/{id}` currently hard-deletes the route and its dependent records.
- `Comment` has `ModeratedAt` and `ModeratedBy`, but does not have `DeletedAt` or `DeletedBy`.

Before implementing petition approval, the route deletion semantics and the comment soft-delete representation need to be aligned with this lifecycle.
