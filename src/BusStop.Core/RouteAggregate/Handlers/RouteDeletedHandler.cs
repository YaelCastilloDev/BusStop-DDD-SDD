namespace BusStop.Core.RouteAggregate.Events;

public class RouteDeletedHandler(ILogger<RouteDeletedHandler> logger) : INotificationHandler<RouteDeletedEvent>
{
  public ValueTask Handle(RouteDeletedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Route {RouteId} was deleted", notification.RouteId);
    return ValueTask.CompletedTask;
  }
}
