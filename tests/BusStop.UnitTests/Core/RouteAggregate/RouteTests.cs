using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Events;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.RouteAggregate;

public class RouteTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = Route.Create("Line A", 1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe("Line A");
        result.Value.CreatedById.Value.ShouldBe(1);
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyName()
    {
        var result = Route.Create("", 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.EmptyName));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceName()
    {
        var result = Route.Create("   ", 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.EmptyName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenInvalidCreatedById(long createdById)
    {
        var result = Route.Create("Line A", createdById);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.InvalidCreatedBy));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = Route.Create("", 0);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.EmptyName));
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.InvalidCreatedBy));
    }

    [Fact]
    public void Delete_Succeeds_WhenNotDeleted()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        var result = route.Delete(new UserId(1));

        result.IsSuccess.ShouldBeTrue();
        route.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_ReturnsError_WhenAlreadyDeleted()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;
        route.Delete(new UserId(1));

        var result = route.Delete(new UserId(2));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.AlreadyDeleted));
    }

    [Fact]
    public void Delete_Throws_WhenNullDeletedBy()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        Should.Throw<ArgumentNullException>(() => route.Delete(null!));
    }

    [Fact]
    public void IsDeleted_ReturnsTrue_AfterDelete()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        route.IsDeleted.ShouldBeFalse();
        route.Delete(new UserId(1));
        route.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void UpdateName_Throws_WhenNullName()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        Should.Throw<ArgumentNullException>(() => route.UpdateName(null!));
    }

    [Fact]
    public void UpdateName_Succeeds_WhenValidName()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        var result = route.UpdateName(new RouteName("Line B"));

        result.IsSuccess.ShouldBeTrue();
        route.Name.Value.ShouldBe("Line B");
    }

    [Fact]
    public void Moderate_Succeeds_WhenNotModerated()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        var result = route.Moderate(new UserId(5));

        result.IsSuccess.ShouldBeTrue();
        route.IsModerated.ShouldBeTrue();
        route.IsDeleted.ShouldBeFalse();
        route.ModeratedBy.ShouldBe(5);
        route.ModeratedAt.ShouldNotBeNull();
        route.DeletedBy.ShouldBeNull();
        route.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public void Moderate_ReturnsError_WhenAlreadyModerated()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;
        route.Moderate(new UserId(5));

        var result = route.Moderate(new UserId(6));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(RouteErrors.AlreadyModerated));
    }

    [Fact]
    public void Moderate_RaisesRouteModeratedEvent()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        route.Moderate(new UserId(5));

        route.DomainEvents.ShouldContain(e => e is RouteModeratedEvent);
        var moderatedEvent = route.DomainEvents.OfType<RouteModeratedEvent>().Single();
        moderatedEvent.RouteId.ShouldBe(route.Id);
        moderatedEvent.ModeratorUserId.ShouldBe(5);
    }

    [Fact]
    public void Moderate_Throws_WhenNullModeratedBy()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        Should.Throw<ArgumentNullException>(() => route.Moderate(null!));
    }

    [Fact]
    public void IsModerated_ReturnsTrue_AfterModerate()
    {
        var routeResult = Route.Create("Line A", 1);
        var route = routeResult.Value;

        route.IsModerated.ShouldBeFalse();
        route.IsDeleted.ShouldBeFalse();
        route.Moderate(new UserId(5));
        route.IsModerated.ShouldBeTrue();
        route.IsDeleted.ShouldBeFalse();
    }
}
