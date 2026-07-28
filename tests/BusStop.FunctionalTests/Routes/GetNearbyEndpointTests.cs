using System.Net;
using System.Net.Http.Json;
using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Infrastructure.Data;
using BusStop.Web.Routes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace BusStop.FunctionalTests.Routes;

[Collection("Sequential")]
public class GetNearbyEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public GetNearbyEndpointTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNearby_ReturnsRoutes_WhenWithinInitialRadius()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("testuser", "test@example.com").Value;
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var route = Route.Create("Downtown Express", user.Id).Value;
        db.Routes.Add(route);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 40.7128, -74.0060 (New York City)
        var stop = Stop.Create("Central Station", 40.7128, -74.0060, route.Id).Value;
        db.Stops.Add(stop);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        // Querying exactly at the stop location
        var response = await _client.GetAsync($"/routes/nearby?latitude=40.7128&longitude=-74.0060", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NearbyRoutesResponse>(TestContext.Current.CancellationToken);
        
        result.ShouldNotBeNull();
        result.IsClosestMatchOnly.ShouldBeFalse();
        result.Routes.ShouldNotBeEmpty();
        result.Routes.First().Name.ShouldBe("Downtown Express");
    }

    [Fact]
    public async Task GetNearby_ReturnsClosestRoute_WhenOutsideInitialRadiusButWithinFallback()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("testuser2", "test2@example.com").Value;
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var route = Route.Create("Uptown Local", user.Id).Value;
        db.Routes.Add(route);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stop = Stop.Create("North Station", 40.7500, -74.0060, route.Id).Value;
        db.Stops.Add(stop);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        // Querying from ~10km away (40.8000, -74.0060)
        var response = await _client.GetAsync($"/routes/nearby?latitude=40.8000&longitude=-74.0060", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NearbyRoutesResponse>(TestContext.Current.CancellationToken);
        
        result.ShouldNotBeNull();
        result.IsClosestMatchOnly.ShouldBeTrue();
        result.Routes.ShouldNotBeEmpty();
        result.Routes.First().Name.ShouldBe("Uptown Local");
    }

    [Fact]
    public async Task GetNearby_ReturnsEmpty_WhenOutsideFallbackRadius()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("testuser3", "test3@example.com").Value;
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var route = Route.Create("Far Away Route", user.Id).Value;
        db.Routes.Add(route);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 40.7128, -74.0060 (New York City)
        var stop = Stop.Create("Far Station", 40.7128, -74.0060, route.Id).Value;
        db.Stops.Add(stop);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        // Querying from London (51.5074, -0.1278) - very far away
        var response = await _client.GetAsync($"/routes/nearby?latitude=51.5074&longitude=-0.1278", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NearbyRoutesResponse>(TestContext.Current.CancellationToken);
        
        result.ShouldNotBeNull();
        result.IsClosestMatchOnly.ShouldBeFalse();
        result.Routes.ShouldBeEmpty();
    }
}
