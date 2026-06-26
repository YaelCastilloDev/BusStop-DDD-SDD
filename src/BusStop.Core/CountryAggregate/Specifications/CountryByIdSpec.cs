namespace BusStop.Core.CountryAggregate.Specifications;

public sealed class CountryByIdSpec : Specification<Country>
{
  public CountryByIdSpec(long id) =>
    Query.Where(c => c.Id == id);
}
