using BusStop.Core.StopAggregate.Events;

namespace BusStop.Core.StopAggregate.Handlers;

public class StopDeletedHandler(ILogger<StopDeletedHandler> logger) : INotificationHandler<StopDeletedEvent>
{
  public ValueTask Handle(StopDeletedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Stop {StopId} was deleted", notification.StopId);
    return ValueTask.CompletedTask;
  }
}
