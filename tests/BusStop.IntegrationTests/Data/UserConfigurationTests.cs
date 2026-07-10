using BusStop.Core.UserAggregate;

namespace BusStop.IntegrationTests.Data;

public class UserConfigurationTests : IntegrationTestBase
{
  public UserConfigurationTests(PostgreSqlFixture fixture) : base(fixture) { }

  [Fact]
  public async Task Create_PersistsCorrectly_WithValidData()
  {
    var user = User.Create("test@example.com", "ext-123").Value;
    DbContext.Users.Add(user);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var saved = await DbContext.Users.FirstOrDefaultAsync(Current.CancellationToken);
    saved.ShouldNotBeNull();
    saved.Email.ShouldBe("test@example.com");
    saved.ExternalId.ShouldBe("ext-123");
    saved.Username.ShouldBeNull();
    saved.CountryId.ShouldBeNull();
  }

  [Fact]
  public async Task CompleteOnboarding_UpdatesProfile_WithValidData()
  {
    var user = User.Create("user@example.com", "ext-456").Value;
    DbContext.Users.Add(user);
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    user.CompleteOnboarding(new Username("onboardeduser"), 1).IsSuccess.ShouldBeTrue();
    await DbContext.SaveChangesAsync(Current.CancellationToken);

    var saved = await DbContext.Users.FirstOrDefaultAsync(Current.CancellationToken);
    saved.ShouldNotBeNull();
    saved.Username.ShouldNotBeNull();
    saved.Username.Value.ShouldBe("onboardeduser");
    saved.CountryId.ShouldBe(1);
  }
}
