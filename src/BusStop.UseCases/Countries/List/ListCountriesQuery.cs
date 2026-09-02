namespace BusStop.UseCases.Countries.List;

public sealed record ListCountriesQuery : IQuery<Result<IEnumerable<CountryResponse>>>;
