using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.Register;

public sealed class OnboardingHandler(
  IRepository<User> repository,
  IReadRepository<Country> countryRepository) : ICommandHandler<OnboardingCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(OnboardingCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result<UserResponse>.Unauthorized("Authentication required.");

    var user = await repository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result<UserResponse>.NotFound("User not found. Please register first.");

    var country = await countryRepository.FirstOrDefaultAsync(new CountryByIdSpec(request.CountryId), cancellationToken);
    if (country is null)
      return Result<UserResponse>.Error("Country not found.");

    var onboardingResult = user.CompleteOnboarding(new Username(request.Username), request.CountryId);
    if (!onboardingResult.IsSuccess)
      return Result<UserResponse>.Error(new ErrorList(onboardingResult.Errors));

    await repository.UpdateAsync(user, cancellationToken);

    return new UserResponse(user.Id, user.Username?.Value, user.Email, user.ExternalId, user.CreatedAt, user.CountryId);
  }
}
