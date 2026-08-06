using BusStop.Core.CommentAggregate;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate;
using BusStop.Infrastructure.Data.Config;
using Npgsql;

namespace BusStop.IntegrationTests.Routes.Delete;

// SPEC-TransitCatalog-RouteDeletionCascade
public class RouteDeletionCascadeTests : IntegrationTestBase
{
    public RouteDeletionCascadeTests(PostgreSqlFixture fixture) : base(fixture) { }

    private async Task<(Route route, List<Stop> stops, List<Comment> comments, ModerationAction action)> SeedRouteWithDependentsAsync()
    {
        var route = Route.Create("Line A", 10).Value;
        DbContext.Routes.Add(route);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        typeof(Ardalis.SharedKernel.EntityBase<long>).GetProperty("Id")!.SetValue(route, route.Id);

        var stop1 = Stop.Create("Stop 1", 40.0, -74.0, route.Id).Value;
        var stop2 = Stop.Create("Stop 2", 41.0, -73.0, route.Id).Value;
        DbContext.Stops.AddRange(stop1, stop2);

        var comment1 = Comment.Create("Nice route", 10, route.Id).Value;
        var comment2 = Comment.Create("Very useful", 10, route.Id).Value;
        DbContext.Comments.AddRange(comment1, comment2);

        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        typeof(Ardalis.SharedKernel.EntityBase<long>).GetProperty("Id")!.SetValue(stop1, stop1.Id);
        typeof(Ardalis.SharedKernel.EntityBase<long>).GetProperty("Id")!.SetValue(stop2, stop2.Id);
        typeof(Ardalis.SharedKernel.EntityBase<long>).GetProperty("Id")!.SetValue(comment1, comment1.Id);
        typeof(Ardalis.SharedKernel.EntityBase<long>).GetProperty("Id")!.SetValue(comment2, comment2.Id);

        DbContext.TargetTypes.Add(new TargetTypeLookup { Id = 1, Name = "Comment" });
        DbContext.TargetTypes.Add(new TargetTypeLookup { Id = 2, Name = "Route" });
        DbContext.ModerationCategories.Add(new ModerationCategoryLookup { Id = 1, Name = "HateSpeech" });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var actionResult = ModerationAction.Create(TargetType.Route, route.Id, 10, 20, ModerationCategory.HateSpeech, "Test reason");
        var action = actionResult.Value;
        DbContext.ModerationActions.Add(action);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (route, [stop1, stop2], [comment1, comment2], action);
    }

    [Fact]
    public async Task HardDelete_RemovesStopsFromDb()
    {
        var (route, stops, _, _) = await SeedRouteWithDependentsAsync();

        DbContext.Routes.Remove(route);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var remainingStops = await DbContext.Stops
            .IgnoreQueryFilters()
            .Where(s => s.RouteId == route.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remainingStops.ShouldBeEmpty();
    }

    [Fact]
    public async Task HardDelete_RemovesCommentsFromDb()
    {
        var (route, _, comments, _) = await SeedRouteWithDependentsAsync();

        DbContext.Routes.Remove(route);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var remainingComments = await DbContext.Comments
            .IgnoreQueryFilters()
            .Where(c => c.RouteId == route.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remainingComments.ShouldBeEmpty();
    }

    [Fact]
    public async Task HardDelete_PreservesModerationActions()
    {
        var (route, _, _, action) = await SeedRouteWithDependentsAsync();

        DbContext.Routes.Remove(route);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var survivingAction = await DbContext.ModerationActions
            .FirstOrDefaultAsync(a => a.Id == action.Id, TestContext.Current.CancellationToken);
        survivingAction.ShouldNotBeNull();
        survivingAction.TargetId.ShouldBe(route.Id);
    }

    [Fact]
    public async Task FkCascade_FiresOnDirectSqlDelete()
    {
        var (route, _, _, _) = await SeedRouteWithDependentsAsync();

        await using var conn = new NpgsqlConnection(_fixture.GetConnectionString().Replace("Database=busstop_test", $"Database={DatabaseName}"));
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM routes WHERE \"Id\" = {route.Id}";
        await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        var remainingStops = await DbContext.Stops
            .IgnoreQueryFilters()
            .Where(s => s.RouteId == route.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remainingStops.ShouldBeEmpty();

        var remainingComments = await DbContext.Comments
            .IgnoreQueryFilters()
            .Where(c => c.RouteId == route.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remainingComments.ShouldBeEmpty();
    }

    [Fact]
    public async Task FkConstraint_RejectsInvalidStopRouteId()
    {
        var stop = Stop.Create("Orphan Stop", 40.0, -74.0, 999999).Value;
        DbContext.Stops.Add(stop);

        var ex = await Should.ThrowAsync<DbUpdateException>(async () =>
            await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
        ex.InnerException.ShouldBeOfType<PostgresException>();
    }
}