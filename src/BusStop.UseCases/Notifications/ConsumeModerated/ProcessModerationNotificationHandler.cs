using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace BusStop.UseCases.Notifications.ConsumeModerated;

public class ProcessModerationNotificationHandler(
  INotificationRepository notificationRepository,
  IReadRepository<User> userRepository,
  IEmailSender emailSender,
  ILogger<ProcessModerationNotificationHandler> logger)
  : ICommandHandler<ProcessModerationNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(ProcessModerationNotificationCommand request, CancellationToken cancellationToken)
  {
    if (request.UserId <= 0)
      return Result.NotFound("User not found.");

    logger.LogInformation("Processing moderation notification for User {UserId}", request.UserId);

    var userResult = await userRepository.FindRequiredAsync(
      new UserByIdSpec(new UserId(request.UserId)),
      "User not found.",
      cancellationToken);

    if (!userResult.IsSuccess)
    {
      logger.LogWarning("User {UserId} not found when processing moderation notification", request.UserId);
      return Result.NotFound("User not found");
    }

    var user = userResult.Value;

    var title = $"Your {request.TargetType} was moderated";
    var message = $"Your {request.TargetType.ToString().ToLower()} (ID: {request.TargetId}) was moderated for {request.Category}. Reason: {request.Reason}";

    var notification = new Notification(request.UserId, title, message);
    await notificationRepository.AddAsync(notification, cancellationToken);
    await emailSender.SendEmailAsync(user.Email, title, message, cancellationToken);

    return Result.Success();
  }
}
