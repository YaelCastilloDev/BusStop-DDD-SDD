# SPEC-TransitCatalog-RouteDeletionCascade

## Bounded Context Owner
TransitCatalog

## Problem Statement
When a route is hard-deleted from the database, all dependent records (stops, comments, and their JSONB reactions) must be physically removed to prevent orphaned data. ModerationActions referencing the route must survive as an audit trail. The existing soft-delete (moderation) flow remains untouched for future notification/email features.

## Domain Invariants
- A route can be hard-deleted only by an authenticated user with the `Curator` role.
- Hard-deleting a route physically removes the route row and all dependent rows (stops, comments) via database-level FK CASCADE.
- ModerationActions are never cascade-deleted; they reference routes polymorphically (`TargetType` + `TargetId`) with no FK constraint.
- `RouteDeletedEvent` is published BEFORE the hard-delete via `IPublisher` (not via `EventDispatchInterceptor`, which cannot dispatch from detached/deleted entities).
- `Comment.Moderate()` is NOT used for cascade deletion. Moderation is a separate operator action that affects users' behavior history (future feature). Route deletion cascade removes comment rows physically via DB CASCADE.

## Two Route Lifecycle End States

### Soft-delete (Moderation)
- **Endpoint:** `PATCH /routes/{id}/moderate` (roles: SubAdmin, Admin)
- **Domain method:** `Route.Moderate(UserId)` — sets `ModeratedAt`/`ModeratedBy`
- **Event:** `RouteModeratedEvent` dispatched via `EventDispatchInterceptor` (post-SaveChanges)
- **Future use:** email/notifications to route creator
- **No data loss:** Route row remains in DB, excluded by query filter

### Hard delete
- **Endpoint:** `DELETE /routes/{id}` (roles: Curator)
- **Handler action:** `repository.DeleteAsync(route)` — physical row removal
- **Event:** `RouteDeletedEvent` (with `DeletedByUserId`) published via `IPublisher.Publish` BEFORE delete
- **Cascade:** DB-level `ON DELETE CASCADE` on `stops.RouteId` and `comments.RouteId` FKs auto-removes dependent rows
- **Audit trail:** ModerationActions survive (no FK to routes)

### `Route.Delete()` (soft-via-DeletedAt)
- Stays on the domain aggregate for future flexibility (potential soft-delete/restore workflow).
- Currently NOT called by any handler.
- Sets `DeletedAt`/`DeletedBy` + registers `RouteDeletedEvent` via interceptor.
- Documented as unused in the current hard-delete flow.

## Use-Case Slice Path
- `UseCases/Routes/Delete/` (existing — modified to hard-delete)

## Layer File Checklist
- **Core**: `RouteAggregate/Events/RouteDeletedEvent.cs` (enhanced with `DeletedByUserId`), `RouteAggregate/Handlers/RouteDeletedHandler.cs` (updated log).
- **UseCases**: `Routes/Delete/DeleteRouteHandler.cs` (modified: hard-delete + event publish).
- **Infrastructure**: `Data/Config/StopConfiguration.cs` (add index), `Data/Config/CommentConfiguration.cs` (add index), new migration `AddRouteCascadeFk`.
- **Web**: No changes (existing `Routes/Delete.cs` endpoint contract unchanged).

## Command/Query and Endpoint Impact
- `DELETE /routes/{id}` -> `DeleteRouteCommand` -> `DeleteRouteHandler` (modified)

## Event Impact
- Published: `RouteDeletedEvent` (enhanced with `DeletedByUserId`).
- Consumed: `RouteDeletedHandler` (logging only — future: notifications/audit).

## Acceptance Criteria
- AC1: `DELETE /routes/{id}` hard-deletes the route row from the `routes` table.
- AC2: All stops with `RouteId = route.Id` are hard-deleted via DB CASCADE.
- AC3: All comments with `RouteId = route.Id` are hard-deleted via DB CASCADE; their JSONB `reactions` column is removed with the comment row.
- AC4: ModerationActions referencing the route (via `TargetType=Route, TargetId=routeId`) are NOT deleted or modified.
- AC5: `RouteDeletedEvent` is published via `IPublisher` BEFORE the delete, carrying `RouteId` and `DeletedByUserId`.
- AC6: DB FK constraint `fk_stops_route` exists: `stops."RouteId" -> routes."Id" ON DELETE CASCADE`.
- AC7: DB FK constraint `fk_comments_route` exists: `comments."RouteId" -> routes."Id" ON DELETE CASCADE`.
- AC8: Index `IX_stops_RouteId` exists.
- AC9: Index `IX_comments_RouteId` exists.
- AC10: Handler returns 204 No Content on success; 404 if route not found; auth check still applies (Curator role).
- AC11: If the DB delete fails (FK constraint, connection error), handler returns `Result.Error` or throws.
- AC12: `DeleteRouteHandler` calls `repository.DeleteAsync`, NOT `repository.UpdateAsync`. It does NOT call `route.Delete()` (soft-delete method).

## Rollout and Rollback Considerations
- Migration adds FK constraints with CASCADE; existing data with orphaned stops/comments must be cleaned before applying.
- Rollback: Drop FK constraints + indexes via the migration's `Down` method.
- The `RouteDeletedEvent` signature change is additive (new constructor parameter); only consumer is `RouteDeletedHandler` (updated in same change).