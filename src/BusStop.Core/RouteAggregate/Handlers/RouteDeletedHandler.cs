using BusStop.Core.RouteAggregate.Events;

namespace BusStop.Core.RouteAggregate.Handlers;

public class RouteDeletedHandler(ILogger<RouteDeletedHandler> logger) : INotificationHandler<RouteDeletedEvent>
{
  public ValueTask Handle(RouteDeletedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Route {RouteId} was deleted", notification.RouteId);
    return ValueTask.CompletedTask;
  }
}
