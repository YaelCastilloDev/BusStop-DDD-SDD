using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.CountryAggregate;
using BusStop.Core.CountryAggregate.Specifications;
using BusStop.Core.Errors;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.UseCases.Users.Onboarding;

namespace BusStop.UnitTests.UseCases.Users.Onboarding;

public class OnboardingHandlerTests
{
    private readonly IRepository<User> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IReadRepository<Country> _countryRepository;
    private readonly OnboardingHandler _handler;

    public OnboardingHandlerTests()
    {
        _repository = Substitute.For<IRepository<User>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _countryRepository = Substitute.For<IReadRepository<Country>>();
        _handler = new OnboardingHandler(_repository, _currentUser, _countryRepository);
    }

    [Fact]
    public async Task Handle_ShouldCheckUsernameUniqueness()
    {
        var command = new OnboardingCommand("duplicate_name", 1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var user = User.Create("test@example.com", "keycloak-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        var otherUser = User.Create("other@example.com", "other-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(otherUser, 2L);
        otherUser.CompleteOnboarding(new Username("duplicate_name"), 1);

        var country = Country.Create("USA", "US").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(country, 1L);

        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByUsernameSpec>(), Arg.Any<CancellationToken>())
            .Returns(otherUser);
        _countryRepository.FirstOrDefaultAsync(Arg.Any<CountryByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(country);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Username is already taken."));
    }

    [Fact]
    public async Task Handle_Succeeds_WhenUsernameIsUnique()
    {
        var command = new OnboardingCommand("unique_name", 1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var user = User.Create("test@example.com", "keycloak-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        var country = Country.Create("USA", "US").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(country, 1L);

        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByUsernameSpec>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _countryRepository.FirstOrDefaultAsync(Arg.Any<CountryByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(country);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }
}
