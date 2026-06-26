using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;

namespace BusStop.UseCases.Countries.List;

public sealed class ListCountriesHandler(IReadRepository<Country> repository) : IQueryHandler<ListCountriesQuery, Result<IEnumerable<CountryResponse>>>
{
  public async ValueTask<Result<IEnumerable<CountryResponse>>> Handle(ListCountriesQuery request, CancellationToken cancellationToken)
  {
    var spec = new CountryAllSpec();
    var countries = await repository.ListAsync(spec, cancellationToken);

    var responses = countries.Select(c => new CountryResponse(c.Id, c.Name, c.IsoCode)).ToList();

    return responses;
  }
}
