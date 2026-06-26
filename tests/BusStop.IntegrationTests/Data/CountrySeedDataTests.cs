using BusStop.Core.CountryAggregate;
using BusStop.Infrastructure.Data;

namespace BusStop.IntegrationTests.Data;

public class CountrySeedDataTests : IntegrationTestBase
{
    [Fact]
    public async Task SeedData_PopulatesCountries()
    {
        await SeedData.InitializeAsync(DbContext);

        var count = await DbContext.Countries.CountAsync(TestContext.Current.CancellationToken);
        count.ShouldBeGreaterThan(200); // 235 countries expected

        var us = await DbContext.Countries.FirstOrDefaultAsync(
            c => c.IsoCode == "US", TestContext.Current.CancellationToken);
        us.ShouldNotBeNull();
        us.Name.ShouldBe("United States");

        var es = await DbContext.Countries.FirstOrDefaultAsync(
            c => c.IsoCode == "ES", TestContext.Current.CancellationToken);
        es.ShouldNotBeNull();
        es.Name.ShouldBe("Spain");
    }

    [Fact]
    public async Task SeedData_IsIdempotent()
    {
        await SeedData.InitializeAsync(DbContext);
        var firstCount = await DbContext.Countries.CountAsync(TestContext.Current.CancellationToken);

        await SeedData.InitializeAsync(DbContext);
        var secondCount = await DbContext.Countries.CountAsync(TestContext.Current.CancellationToken);

        secondCount.ShouldBe(firstCount);
    }
}
