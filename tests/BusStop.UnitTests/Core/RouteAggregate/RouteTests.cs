using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.RouteAggregate;
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
}
