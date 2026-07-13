using BusStop.Core.Errors;
using BusStop.Core.UserAggregate.Events;

namespace BusStop.Core.UserAggregate;

public class User : EntityBase<long>, IAggregateRoot
{
    public Username? Username { get; private set; }
    public string Email { get; private set; }
    public long? CountryId { get; private set; }
    public string? ExternalId { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    private User(string email, string? externalId)
    {
        Guard.Against.NullOrWhiteSpace(email, nameof(email));

        Email = email;
        ExternalId = externalId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(string email, string externalId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(UserErrors.EmptyEmail);
        if (string.IsNullOrWhiteSpace(externalId))
            errors.Add(UserErrors.EmptyExternalId);

        if (errors.Count > 0)
            return Result<User>.Error(new ErrorList(errors));

        var user = new User(email, externalId);
        user.RegisterDomainEvent(new UserRegisteredEvent(user.Email, user.ExternalId!));
        return Result<User>.Success(user);
    }

    public Result CompleteOnboarding(Username username, long countryId)
    {
        Guard.Against.Null(username, nameof(username));

        if (countryId <= 0)
            return Result.Error(new ErrorList([UserErrors.InvalidCountryId]));

        Username = username;
        CountryId = countryId;
        return Result.Success();
    }

    public Result UpdateUsername(Username newUsername)
    {
        Guard.Against.Null(newUsername, nameof(newUsername));
        Username = newUsername;
        return Result.Success();
    }

    public Result UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            return Result.Error(new ErrorList([UserErrors.EmptyEmail]));

        Email = newEmail;
        return Result.Success();
    }
}
