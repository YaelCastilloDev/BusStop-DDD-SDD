namespace BusStop.Core.CountryAggregate.Specifications;

public sealed class CountryAllSpec : Specification<Country>
{
  public CountryAllSpec() =>
    Query.OrderBy(c => c.Name);
}
