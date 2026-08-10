using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.ModerationActionAggregate.Events;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Routes.Moderate;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Routes.Moderate;

// SPEC-TransitCatalog-ModerationAction
public class ModerateRouteHandlerTests
{
    private readonly IRepository<Route> _routeRepository;
    private readonly IRepository<ModerationAction> _moderationActionRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ModerateRouteHandler _handler;

    public ModerateRouteHandlerTests()
    {
        _routeRepository = Substitute.For<IRepository<Route>>();
        _moderationActionRepository = Substitute.For<IRepository<ModerationAction>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new ModerateRouteHandler(_routeRepository, _moderationActionRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenValidData()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.InappropriateContent, "Inappropriate route") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);
        _moderationActionRepository.AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _moderationActionRepository.Received(1).AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
        await _routeRepository.Received(1).UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Succeeds_AndRegistersModerationActionRecordedEvent()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.InappropriateContent, "Inappropriate route") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        ModerationAction? capturedAction = null;
        _moderationActionRepository.AddAsync(Arg.Do<ModerationAction>(a => capturedAction = a), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        capturedAction.ShouldNotBeNull();
        var domainEvent = capturedAction.DomainEvents.OfType<ModerationActionRecordedEvent>().Single();
        domainEvent.TargetType.ShouldBe(TargetType.Route);
        domainEvent.TargetId.ShouldBe(route.Id);
        domainEvent.UserId.ShouldBe(route.CreatedById.Value);
        domainEvent.IssuedByUserId.ShouldBe(5);
        domainEvent.Category.ShouldBe(ModerationCategory.InappropriateContent);
        domainEvent.Reason.ShouldBe("Inappropriate route");
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenRouteMissing()
    {
        var command = new ModerateRouteCommand(99, ModerationCategory.Spam, "Spam route") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((Route?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _moderationActionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.Spam, "Reason") { Sub = "unknown-sub" };
        _currentUser.Id.Returns(0L);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRouteAlreadyModerated()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.HateSpeech, "Hate speech") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);
        route.Moderate(new UserId(5)); // Already moderated/deleted

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        await _moderationActionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
        await _routeRepository.DidNotReceive().UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenModerationActionCreationFails()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.HateSpeech, "") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        await _moderationActionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
        await _routeRepository.DidNotReceive().UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotPersistModerationAction_WhenRouteUpdateFails()
    {
        var command = new ModerateRouteCommand(1, ModerationCategory.InappropriateContent, "Inappropriate route") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var route = Route.Create("Line A", 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(route, 1L);

        _routeRepository.FirstOrDefaultAsync(Arg.Any<RouteByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(route);
        _moderationActionRepository.AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());
        _routeRepository.When(x => x.UpdateAsync(route, Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Database failure"));

        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Database failure");

        await _moderationActionRepository.Received(1).AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
    }
}
