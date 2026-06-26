using Ardalis.Result;
using Mediator;

namespace BusStop.UseCases.Notifications.ConsumeModerated;

public sealed record ProcessModerationNotificationCommand(long UserId, long CommentId, string ModerationReason) : ICommand<Result>;
