using BusStop.Infrastructure.Data.Queries;

namespace BusStop.UnitTests.Integrations;

public class NearbyRoutesQueryServiceTests
{
    [Fact]
    public void BuildQuery_ShouldFilterModeratedRoutes()
    {
        var (sql, _) = NearbyRoutesQueryService.BuildQuery(0, 0, 100, limit: null);

        sql.ShouldContain("ModeratedAt", Case.Insensitive,
            "raw SQL must filter moderated routes since SqlQueryRaw bypasses RouteConfiguration.HasQueryFilter");
    }

    [Fact]
    public void BuildQuery_ShouldContainDeletedAtFilters()
    {
        var (sql, _) = NearbyRoutesQueryService.BuildQuery(0, 0, 100, limit: null);

        sql.ShouldContain("r.\"DeletedAt\" IS NULL");
        sql.ShouldContain("s.\"DeletedAt\" IS NULL");
    }

    [Fact]
    public void BuildQuery_ShouldNot_Interpolate_Limit_Parameter()
    {
        var (sql, parameters) = NearbyRoutesQueryService.BuildQuery(0, 0, 100, limit: 5);

        sql.ShouldNotContain("LIMIT 5");
        sql.ShouldContain("LIMIT {3}");
        parameters.ShouldContain(5);
    }

    [Fact]
    public void BuildQuery_ShouldOmit_LimitPlaceholder_WhenNoLimit()
    {
        var (sql, parameters) = NearbyRoutesQueryService.BuildQuery(0, 0, 100, limit: null);

        sql.ShouldNotContain("LIMIT", Case.Insensitive);
        parameters.Length.ShouldBe(3);
    }

    [Fact]
    public void BuildQuery_ShouldContain_4Parameters_WhenLimitSpecified()
    {
        var (_, parameters) = NearbyRoutesQueryService.BuildQuery(0, 0, 100, limit: 1);

        parameters.Length.ShouldBe(4);
    }
}
