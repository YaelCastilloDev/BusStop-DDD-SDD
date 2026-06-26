using Ardalis.Result;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Interfaces;
using BusStop.Core.UserAggregate;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BusStop.UseCases.Notifications.ConsumeModerated;

public class ProcessModerationNotificationHandler(
  IRepository<UserNotification> repository,
  IReadRepository<User> userRepository,
  IEmailSender emailSender,
  ILogger<ProcessModerationNotificationHandler> logger)
  : ICommandHandler<ProcessModerationNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(ProcessModerationNotificationCommand request, CancellationToken cancellationToken)
  {
    logger.LogInformation("Processing moderation notification for User {UserId}", request.UserId);

    var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user is null)
    {
      logger.LogWarning("User {UserId} not found when processing moderation notification", request.UserId);
      return Result.NotFound("User not found");
    }

    var title = "Your comment was moderated";
    var message = $"Your comment (ID: {request.CommentId}) was moderated. Reason: {request.ModerationReason}";

    var notification = UserNotification.Create(request.UserId, title, message);
    await repository.AddAsync(notification, cancellationToken);

    // Send email
    await emailSender.SendEmailAsync(user.Email, title, message, cancellationToken);

    return Result.Success();
  }
}
