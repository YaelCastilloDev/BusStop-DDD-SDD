namespace BusStop.FunctionalTests.Auth;

[Collection("Sequential")]
public class KeycloakAuthTests : IClassFixture<KeycloakFixture>
{
    private readonly KeycloakFixture _keycloak;
    private readonly HttpClient _httpClient = new();

    public KeycloakAuthTests(KeycloakFixture keycloak)
    {
        _keycloak = keycloak;
    }

    [Fact]
    public async Task RealmEndpoint_IsAccessible()
    {
        var response = await _httpClient.GetAsync($"{_keycloak.RealmUrl}", CancellationToken.None);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task OidcDiscoveryEndpoint_ReturnsValidConfig()
    {
        var response = await _httpClient.GetAsync(
            $"{_keycloak.RealmUrl}/.well-known/openid-configuration", CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        body.ShouldContain("token_endpoint");
        body.ShouldContain("issuer");
        body.ShouldContain("authorization_endpoint");
    }

    [Fact]
    public async Task TokenEndpoint_ReturnsAccessToken_ForRegisteredUser()
    {
        var token = await _keycloak.GetTokenAsync("registered1", "password");

        token.ShouldNotBeNull();
        token!.AccessToken.ShouldNotBeNullOrEmpty();
        token.TokenType.ShouldBe("Bearer");
        token.ExpiresIn.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task TokenEndpoint_ReturnsTokens_ForCuratorUser()
    {
        var token = await _keycloak.GetTokenAsync("curator1", "password");

        token.ShouldNotBeNull();
        token!.AccessToken.ShouldNotBeNullOrEmpty();
        token.RefreshToken.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenEndpoint_RejectsInvalidPassword()
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "busstop-api"),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "registered1"),
            new KeyValuePair<string, string>("password", "wrong-password"),
        });

        var response = await client.PostAsync(_keycloak.TokenUrl, content, CancellationToken.None);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }
}
