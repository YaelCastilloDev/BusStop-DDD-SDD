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
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(isoCode);
        return new Country(name, isoCode);
    }

    public void UpdateName(string newName)
    {
        Guard.Against.NullOrWhiteSpace(newName);
        Name = newName;
    }

    public void UpdateIsoCode(string newIsoCode)
    {
        Guard.Against.NullOrWhiteSpace(newIsoCode);
        IsoCode = newIsoCode;
    }
}
