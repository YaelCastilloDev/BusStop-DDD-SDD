using Ardalis.Specification;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Events;
using BusStop.Infrastructure.Handlers;
using BusStop.Infrastructure.Integrations.RabbitMQ;
using MassTransit;

namespace BusStop.UnitTests.Integrations;

public class UserRegisteredIntegrationHandlerTests
{
    [Fact]
    public async Task Handle_SetsUsername_ToEmail_NotActualUsername()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        publishEndpoint.Publish(Arg.Any<UserRegisteredIntegrationEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var userResult = User.Create("test@example.com", "ext-123");
        userResult.IsSuccess.ShouldBeTrue();
        var user = userResult.Value;

        var userReadRepository = Substitute.For<IReadRepository<User>>();
        userReadRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        var handler = new UserRegisteredIntegrationHandler(publishEndpoint, userReadRepository);

        var notification = new UserRegisteredEvent("test@example.com", "ext-123");

        await handler.Handle(notification, CancellationToken.None);

        await publishEndpoint.Received(1).Publish(
            Arg.Is<UserRegisteredIntegrationEvent>(e =>
                e.Username == "test@example.com" &&
                e.Email == "test@example.com" &&
                e.UserId == user.Id),
            Arg.Any<CancellationToken>());

        user.Username.ShouldBeNull();
    }
}
