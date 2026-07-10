using Ardalis.Result;
using BusStop.Core.CountryAggregate;
using BusStop.Core.Errors;

namespace BusStop.UnitTests.Core.CountryAggregate;

public class CountryTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = Country.Create("France", "FR");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("France");
        result.Value.IsoCode.ShouldBe("FR");
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyName()
    {
        var result = Country.Create("", "FR");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyName));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceName()
    {
        var result = Country.Create("   ", "FR");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyName));
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyIsoCode()
    {
        var result = Country.Create("France", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyIsoCode));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceIsoCode()
    {
        var result = Country.Create("France", "   ");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyIsoCode));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = Country.Create("", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyName));
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyIsoCode));
    }

    [Fact]
    public void UpdateName_Succeeds_WhenValidName()
    {
        var countryResult = Country.Create("France", "FR");
        var country = countryResult.Value;

        var result = country.UpdateName("Italia");

        result.IsSuccess.ShouldBeTrue();
        country.Name.ShouldBe("Italia");
    }

    [Fact]
    public void UpdateName_ReturnsError_WhenEmptyName()
    {
        var countryResult = Country.Create("France", "FR");
        var country = countryResult.Value;

        var result = country.UpdateName("");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyName));
        country.Name.ShouldBe("France");
    }

    [Fact]
    public void UpdateIsoCode_Succeeds_WhenValidIsoCode()
    {
        var countryResult = Country.Create("France", "FR");
        var country = countryResult.Value;

        var result = country.UpdateIsoCode("IT");

        result.IsSuccess.ShouldBeTrue();
        country.IsoCode.ShouldBe("IT");
    }

    [Fact]
    public void UpdateIsoCode_ReturnsError_WhenEmptyIsoCode()
    {
        var countryResult = Country.Create("France", "FR");
        var country = countryResult.Value;

        var result = country.UpdateIsoCode("");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CountryErrors.EmptyIsoCode));
        country.IsoCode.ShouldBe("FR");
    }
}
