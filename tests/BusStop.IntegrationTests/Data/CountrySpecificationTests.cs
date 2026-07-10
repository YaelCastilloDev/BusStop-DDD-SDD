using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;

namespace BusStop.IntegrationTests.Data;

public class CountrySpecificationTests : IntegrationTestBase
{
  public CountrySpecificationTests(PostgreSqlFixture fixture) : base(fixture) { }

  [Fact]
  public async Task CountryByIdSpec_ReturnsCorrectCountry()
  {
    var country = Country.Create("Spain", "ES").Value;
    DbContext.Countries.Add(country);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var spec = new CountryByIdSpec(country.Id);
    var result = await DbContext.Countries.WithSpecification(spec).FirstOrDefaultAsync(Current.CancellationToken);

    result.ShouldNotBeNull();
    result.Name.ShouldBe("Spain");
    result.IsoCode.ShouldBe("ES");
  }

  [Fact]
  public async Task CountryAllSpec_ReturnsOrderedByName()
  {
    DbContext.Countries.Add(Country.Create("Zambia", "ZM").Value);
    DbContext.Countries.Add(Country.Create("Argentina", "AR").Value);
    DbContext.Countries.Add(Country.Create("Brazil", "BR").Value);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var spec = new CountryAllSpec();
    var results = await DbContext.Countries.WithSpecification(spec).ToListAsync(Current.CancellationToken);

    results.Count.ShouldBe(3);
    results[0].Name.ShouldBe("Argentina");
    results[1].Name.ShouldBe("Brazil");
    results[2].Name.ShouldBe("Zambia");
  }
}
