using BusStop.Core.UserAggregate;
using BusStop.Infrastructure.Data;
using BusStop.UseCases.Users;
using BusStop.UseCases.Users.Register;

namespace BusStop.IntegrationTests.UseCases;

// SPEC-IdentityAccess-RegisterFlow
public class RegisterUserHandlerTests : IntegrationTestBase
{
  private EfRepository<User> _userRepository = null!;
  private RegisterUserHandler _handler = null!;

  public RegisterUserHandlerTests(PostgreSqlFixture fixture) : base(fixture) { }

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();
    _userRepository = new EfRepository<User>(DbContext);
    _handler = new RegisterUserHandler(_userRepository);
  }

  [Fact]
  public async Task RegisterUser_Succeeds_WithValidData()
  {
    var command = new RegisterUserCommand("new@example.com")
    {
      Sub = "kc-sub-newuser"
    };

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Email.ShouldBe("new@example.com");
    result.Value.ExternalId.ShouldBe("kc-sub-newuser");
    result.Value.Username.ShouldBeNull();
    result.Value.CountryId.ShouldBeNull();
  }

  [Fact]
  public async Task RegisterUser_Fails_WhenAlreadyRegistered()
  {
    var existing = User.Create("existing@example.com", "kc-sub-existing");
    await _userRepository.AddAsync(existing, Current.CancellationToken);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var command = new RegisterUserCommand("another@example.com")
    {
      Sub = "kc-sub-existing"
    };

    var result = await _handler.Handle(command, Current.CancellationToken);

    result.IsSuccess.ShouldBeFalse();
    result.Errors.ShouldContain(e => e.Contains("already registered"));
  }

}
