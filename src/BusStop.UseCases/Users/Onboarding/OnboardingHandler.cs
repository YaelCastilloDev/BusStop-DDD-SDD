using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.Onboarding;

// TODO: Deferred — username uniqueness domain invariant is not enforced.
// SPEC-IdentityAccess-RegisterFlow requires "Username is unique across all users" but
// no duplicate-username check exists here. Keycloak only handles email uniqueness.
// A UserByUsernameSpec and guard clause should be added before closing this spec.
public sealed class OnboardingHandler(
  IRepository<User> repository,
  IReadRepository<Country> countryRepository) : ICommandHandler<OnboardingCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(OnboardingCommand request, CancellationToken cancellationToken)
  {
    var userResult = await repository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result<UserResponse>.NotFound("User not found.");
    var user = userResult.Value;

    // TODO: Inconsistency — uses Result.Error("Country not found.") instead of Result.NotFound.
    // Cannot blindly replace with FindRequiredAsync which returns NotFound (contract change).
    // See REFACTOR-DRY-001 Phase 3 — should eventually standardize to NotFound.
    var country = await countryRepository.FirstOrDefaultAsync(new CountryByIdSpec(request.CountryId), cancellationToken);
    if (country is null)
      return Result<UserResponse>.Error("Country not found.");

    var onboardingResult = user.CompleteOnboarding(new Username(request.Username), request.CountryId);
    if (!onboardingResult.IsSuccess)
      return Result<UserResponse>.Error(new ErrorList(onboardingResult.Errors));

    await repository.UpdateAsync(user, cancellationToken);

    return user.ToResponse();
  }
}
