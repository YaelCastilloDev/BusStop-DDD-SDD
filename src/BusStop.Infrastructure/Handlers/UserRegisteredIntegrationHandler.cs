using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Events;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.Infrastructure.Integrations.RabbitMQ;
using MassTransit;
using Mediator;

// TODO: Implement consumer for UserRegisteredIntegrationEvent (Gate 5 — Contract Safety).
//       Currently this event is published but no service consumes it. Intended for
//       future cross-context or external consumers via RabbitMQ.

namespace BusStop.Infrastructure.Handlers;

public sealed class UserRegisteredIntegrationHandler(
    IPublishEndpoint publishEndpoint,
    IReadRepository<User> userReadRepository) : INotificationHandler<UserRegisteredEvent>
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IReadRepository<User> _userReadRepository = userReadRepository;

    public async ValueTask Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.FirstOrDefaultAsync(
            new UserByExternalIdSpec(notification.ExternalId), cancellationToken);

        if (user is null) return;

        var integrationEvent = new UserRegisteredIntegrationEvent
        {
            UserId = user.Id,
            Username = notification.Email,
            Email = notification.Email,
            RegisteredAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
