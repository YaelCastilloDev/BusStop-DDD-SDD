using FastEndpoints;
using FluentValidation;

namespace BusStop.Web.Notifications;

public sealed class DeleteNotificationValidator : Validator<DeleteNotificationRequest>
{
  public DeleteNotificationValidator()
  {
    RuleFor(x => x.Id).GreaterThan(0);
  }
}
