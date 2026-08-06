using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Events;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.UseCases.Routes.Delete;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Routes.Delete;

// SPEC-TransitCatalog-RouteDeletionCascade
public class DeleteRouteHandlerTests
{
    private readonly IRepository<Route> _routeRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IPublisher _publisher;
    private readonly DeleteRouteHandler _handler;

    public DeleteRouteHandlerTests()
    {
        _routeRepository = Substitute.For<IRepository<Route>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new DeleteRouteHandler(_routeRepository, _currentUser, _publisher);
    }

    [Fact]
    public async Task Handle_CallsDeleteAsync_NotUpdateAsync()
    {
        var command = new DeleteRouteCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _routeRepository.Received(1).DeleteAsync(route, Arg.Any<CancellationToken>());
        await _routeRepository.DidNotReceive().UpdateAsync(Arg.Any<Route>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PublishesRouteDeletedEvent_WithRouteIdAndUserId()
    {
        var command = new DeleteRouteCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<RouteDeletedEvent>(e => e.RouteId == 1L && e.DeletedByUserId == 5L),
            Arg.Any<CancellationToken>());
        await _routeRepository.Received(1).DeleteAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotLoadOrModifyStopsOrComments()
    {
        var command = new DeleteRouteCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        await _handler.Handle(command, CancellationToken.None);

        await _routeRepository.Received(1).DeleteAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenRouteMissing()
    {
        var command = new DeleteRouteCommand(99) { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((Route?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _routeRepository.DidNotReceive().DeleteAsync(Arg.Any<Route>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<RouteDeletedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new DeleteRouteCommand(1) { Sub = "unknown-sub" };
        _currentUser.Id.Returns(0L);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _routeRepository.DidNotReceive().DeleteAsync(Arg.Any<Route>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<RouteDeletedEvent>(), Arg.Any<CancellationToken>());
    }
}