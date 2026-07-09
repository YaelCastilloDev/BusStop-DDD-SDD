using BusStop.Core.Exceptions;

namespace BusStop.Core.CountryAggregate;

public class Country : EntityBase<long>, IAggregateRoot
{
    public string Name { get; private set; }
    public string IsoCode { get; private set; }

#pragma warning disable CS8618
    private Country() { }
#pragma warning restore CS8618

    private Country(string name, string isoCode)
    {
        Name = name;
        IsoCode = isoCode;
    }

    public static Country Create(string name, string isoCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Country name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(isoCode))
            throw new DomainValidationException("Country ISO code is required.", nameof(isoCode));
        return new Country(name, isoCode);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainValidationException("New country name is required.", nameof(newName));
        Name = newName;
    }

    public void UpdateIsoCode(string newIsoCode)
    {
        if (string.IsNullOrWhiteSpace(newIsoCode))
            throw new DomainValidationException("New country ISO code is required.", nameof(newIsoCode));
        IsoCode = newIsoCode;
    }
}
