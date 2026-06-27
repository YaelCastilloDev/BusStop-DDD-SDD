# SPEC-NotificationContext-Moderation

## Bounded Context Owner
NotificationContext

## Problem Statement
Users need to be notified when actions occur in the system that affect them, such as when their comment is moderated. We want to avoid slow polling approaches and use real-time push notifications via SignalR, coupled with background event processing via MassTransit.

## Domain Invariants
- A notification belongs to a single user.
- A user can only delete their own notifications.
- The notification message must be immutable once created.

## Use-Case Slice Path
- `UseCases/Notifications/GetMy/`
- `UseCases/Notifications/Delete/`
- `UseCases/Notifications/ConsumeModerated/`

## Layer File Checklist
- **AspireHost**: `AppHost.cs` (RabbitMQ).
- **Core**: `NotificationAggregate/` (UserNotification, UserNotificationId).
- **UseCases**: Commands, Queries, Handlers, Consumers.
- **Infrastructure**: Entity Configurations, EF Core DbContext, ResendEmailSender.
- **Web**: FastEndpoints (`GetMy`, `Delete`), SignalR Hub (`NotificationsHub`).

## Command/Query and Endpoint Impact
- `GET /notifications` -> `GetMyNotificationsQuery`
- `DELETE /notifications/{id}` -> `DeleteNotificationCommand`

## Event Impact
- Consumes: `CommentModeratedIntegrationEvent` (from TransitCatalog).

## Acceptance Criteria
- Given a comment is moderated, when the event is processed, a UserNotification is saved to the database.
- Given a UserNotification is saved, it is pushed to the target user via SignalR.
- Given a UserNotification is processed, an email is sent via the `IEmailSender` (using Resend API, decoupled via RabbitMQ).
- Given a user requests their notifications, they only receive their own notifications.
- Given a user attempts to delete a notification, it is deleted only if they own it; otherwise, it fails or returns NotFound/Forbidden.

## Rollout and Rollback Considerations
- RabbitMQ must be deployed.
- SignalR connections require WebSockets support.
- Rollback: Revert migrations, remove RabbitMQ if needed.
