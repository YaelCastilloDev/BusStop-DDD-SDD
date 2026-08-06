using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;
using BusStop.Core.Errors;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.Onboarding;

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

    var existingUser = await repository.FirstOrDefaultAsync(
      new UserByUsernameSpec(request.Username),
      cancellationToken);

    if (existingUser is not null && existingUser.Id != user.Id)
      return Result<UserResponse>.Error(new ErrorList([UserErrors.UsernameAlreadyTaken]));

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
