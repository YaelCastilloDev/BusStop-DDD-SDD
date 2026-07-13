using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.Onboarding;

// TODO: Deferred — username uniqueness domain invariant is not enforced.
// SPEC-IdentityAccess-RegisterFlow requires "Username is unique across all users" but
// no duplicate-username check exists here. Keycloak only handles email uniqueness.
// A UserByUsernameSpec and guard clause should be added before closing this spec.
public sealed class OnboardingHandler(
  IRepository<User> repository,
  ICurrentUser currentUser,
  IReadRepository<Country> countryRepository) : ICommandHandler<OnboardingCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(OnboardingCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result<UserResponse>.NotFound("User not found.");

    var userResult = await repository.FindRequiredAsync(new UserByIdSpec(new UserId(currentUser.Id)), "User not found.", cancellationToken);
    if (!userResult.IsSuccess)
      return Result<UserResponse>.NotFound("User not found.");
    var user = userResult.Value;

    var countryResult = await countryRepository.FindRequiredAsync(
      new CountryByIdSpec(request.CountryId),
      "Country not found.",
      cancellationToken);

    if (!countryResult.IsSuccess)
      return Result<UserResponse>.NotFound("Country not found.");

    var country = countryResult.Value;

    var onboardingResult = user.CompleteOnboarding(new Username(request.Username), request.CountryId);
    if (!onboardingResult.IsSuccess)
      return Result<UserResponse>.Error(new ErrorList(onboardingResult.Errors));

    await repository.UpdateAsync(user, cancellationToken);

    return user.ToResponse();
  }
}
