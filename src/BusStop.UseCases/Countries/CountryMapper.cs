using BusStop.Core.CountryAggregate;

namespace BusStop.UseCases.Countries;

public static class CountryMapper
{
    public static CountryResponse ToResponse(this Country country) =>
        new(country.Id, country.Name, country.IsoCode);
}
