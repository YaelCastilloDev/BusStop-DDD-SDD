using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.UserAggregate;

public class UserTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = User.Create("test@example.com", "keycloak-sub");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("test@example.com");
        result.Value.ExternalId.ShouldBe("keycloak-sub");
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyEmail()
    {
        var result = User.Create("", "keycloak-sub");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyEmail));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceEmail()
    {
        var result = User.Create("   ", "keycloak-sub");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyEmail));
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyExternalId()
    {
        var result = User.Create("test@example.com", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyExternalId));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceExternalId()
    {
        var result = User.Create("test@example.com", "   ");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyExternalId));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = User.Create("", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyEmail));
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyExternalId));
    }

    [Fact]
    public void CompleteOnboarding_Succeeds_WhenValidInput()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        var result = user.CompleteOnboarding(new Username("john_doe"), 1);

        result.IsSuccess.ShouldBeTrue();
        user.Username!.Value.ShouldBe("john_doe");
        user.CountryId.ShouldBe(1);
    }

    [Fact]
    public void CompleteOnboarding_Throws_WhenNullUsername()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        Should.Throw<ArgumentNullException>(() => user.CompleteOnboarding(null!, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CompleteOnboarding_ReturnsError_WhenInvalidCountryId(long countryId)
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        var result = user.CompleteOnboarding(new Username("john_doe"), countryId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.InvalidCountryId));
    }

    [Fact]
    public void UpdateUsername_Throws_WhenNullUsername()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        Should.Throw<ArgumentNullException>(() => user.UpdateUsername(null!));
    }

    [Fact]
    public void UpdateUsername_Succeeds_WhenValidUsername()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        var result = user.UpdateUsername(new Username("new_username"));

        result.IsSuccess.ShouldBeTrue();
        user.Username!.Value.ShouldBe("new_username");
    }

    [Fact]
    public void UpdateEmail_Succeeds_WhenValidEmail()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        var result = user.UpdateEmail("new@example.com");

        result.IsSuccess.ShouldBeTrue();
        user.Email.ShouldBe("new@example.com");
    }

    [Fact]
    public void UpdateEmail_ReturnsError_WhenEmptyEmail()
    {
        var userResult = User.Create("test@example.com", "keycloak-sub");
        var user = userResult.Value;

        var result = user.UpdateEmail("");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.EmptyEmail));
        user.Email.ShouldBe("test@example.com");
    }
}
