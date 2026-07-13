using BusStop.Core.RouteAggregate.Events;

namespace BusStop.Core.RouteAggregate.Handlers;

public class RouteModeratedHandler(ILogger<RouteModeratedHandler> logger) : INotificationHandler<RouteModeratedEvent>
{
    public ValueTask Handle(RouteModeratedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Route {RouteId} was moderated by moderator {ModeratorUserId}", notification.RouteId, notification.ModeratorUserId);
        return ValueTask.CompletedTask;
    }
}
