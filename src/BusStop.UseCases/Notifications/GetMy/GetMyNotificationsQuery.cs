using Ardalis.Result;
using BusStop.Core.Interfaces;
using Mediator;

namespace BusStop.UseCases.Notifications.GetMy;

public sealed record GetMyNotificationsQuery() : IQuery<Result<IEnumerable<NotificationDto>>>, IRequireAuthenticatedUser
{
  public string? Sub { get; set; }
}
