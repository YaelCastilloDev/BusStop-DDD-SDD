namespace BusStop.Core.Errors;

public static class UserErrors
{
    public const string EmptyEmail = "Email is required.";
    public const string InvalidEmail = "Email format is invalid.";
    public const string EmptyExternalId = "External ID is required.";
    public const string InvalidCountryId = "Country ID must be positive.";
    public const string UsernameAlreadyTaken = "Username is already taken.";
    public const string AlreadyOnboarded = "User has already completed onboarding.";
}
