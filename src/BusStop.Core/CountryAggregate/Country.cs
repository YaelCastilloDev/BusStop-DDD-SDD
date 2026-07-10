using BusStop.Core.Errors;

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
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(isoCode, nameof(isoCode));

        Name = name;
        IsoCode = isoCode;
    }

    public static Result<Country> Create(string name, string isoCode)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(CountryErrors.EmptyName);
        if (string.IsNullOrWhiteSpace(isoCode))
            errors.Add(CountryErrors.EmptyIsoCode);

        if (errors.Count > 0)
            return Result<Country>.Error(new ErrorList(errors));

        return Result<Country>.Success(new Country(name, isoCode));
    }

    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Error(new ErrorList([CountryErrors.EmptyName]));

        Name = newName;
        return Result.Success();
    }

    public Result UpdateIsoCode(string newIsoCode)
    {
        if (string.IsNullOrWhiteSpace(newIsoCode))
            return Result.Error(new ErrorList([CountryErrors.EmptyIsoCode]));

        IsoCode = newIsoCode;
        return Result.Success();
    }
}
