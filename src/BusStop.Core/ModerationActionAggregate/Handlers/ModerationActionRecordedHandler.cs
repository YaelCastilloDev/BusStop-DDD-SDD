using BusStop.Core.ModerationActionAggregate.Events;

namespace BusStop.Core.ModerationActionAggregate.Handlers;

public class ModerationActionRecordedHandler(ILogger<ModerationActionRecordedHandler> logger) : INotificationHandler<ModerationActionRecordedEvent>
{
    public ValueTask Handle(ModerationActionRecordedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Moderation action {ModerationActionId} recorded against {TargetType} {TargetId} for user {UserId} by moderator {IssuedByUserId} (category: {Category})",
            notification.ModerationActionId, notification.TargetType, notification.TargetId, notification.UserId, notification.IssuedByUserId, notification.Category);
        return ValueTask.CompletedTask;
    }
}
