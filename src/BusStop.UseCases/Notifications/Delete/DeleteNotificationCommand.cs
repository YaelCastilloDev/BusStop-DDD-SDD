using Ardalis.Result;
using BusStop.Core.Interfaces;
using Mediator;

namespace BusStop.UseCases.Notifications.Delete;

public sealed record DeleteNotificationCommand(long NotificationId) : ICommand<Result>, IRequireAuthenticatedUser
{
  public string Sub { get; set; } = default!;
}
