using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UseCases.Notifications.ConsumeModerated;

public sealed record ProcessModerationNotificationCommand(
    long UserId,
    TargetType TargetType,
    long TargetId,
    string Reason,
    ModerationCategory Category) : ICommand<Result>;
