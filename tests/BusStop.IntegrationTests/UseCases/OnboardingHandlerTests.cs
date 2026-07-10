using BusStop.Core.CountryAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Infrastructure.Data;
using BusStop.UseCases.Users;
using BusStop.UseCases.Users.Register;

namespace BusStop.IntegrationTests.UseCases;

public class OnboardingHandlerTests : IntegrationTestBase
{
  private readonly EfRepository<User> _userRepository;
  private readonly EfRepository<Country> _countryRepository;
  private readonly OnboardingHandler _handler;

  public OnboardingHandlerTests()
  {
    _userRepository = new EfRepository<User>(DbContext);
    _countryRepository = new EfRepository<Country>(DbContext);
    _handler = new OnboardingHandler(_userRepository, _countryRepository);
  }

  [Fact]
  public async Task Onboarding_Succeeds_WithValidData()
  {
    var user = User.Create("user@example.com", "kc-sub-onboarding").Value;
    await _userRepository.AddAsync(user, Current.CancellationToken);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var country = Country.Create("France", "FR").Value;
    await _countryRepository.AddAsync(country, Current.CancellationToken);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var command = new OnboardingCommand("john", country.Id)
    {
      Sub = "kc-sub-onboarding"
    };

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Username.ShouldBe("john");
    result.Value.CountryId.ShouldBe(country.Id);
    result.Value.Email.ShouldBe("user@example.com");
  }

  [Fact]
  public async Task Onboarding_Fails_WhenUserNotRegistered()
  {
    var command = new OnboardingCommand("nouser", 1)
    {
      Sub = "nonexistent-sub"
    };

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Onboarding_Fails_WithInvalidCountry()
  {
    var user = User.Create("user2@example.com", "kc-sub-invalid-country").Value;
    await _userRepository.AddAsync(user, Current.CancellationToken);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var command = new OnboardingCommand("jane", 99999)
    {
      Sub = "kc-sub-invalid-country"
    };

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeFalse();
    result.Errors.ShouldContain(e => e.Contains("Country not found"));
  }

  [Fact]
  public async Task Onboarding_Fails_WithoutSub()
  {
    var command = new OnboardingCommand("noauth", 1);

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Unauthorized);
  }
}
