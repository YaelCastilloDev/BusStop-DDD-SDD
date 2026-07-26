using System.Net;
using System.Net.Http.Headers;

namespace BusStop.FunctionalTests.Auth;

[Collection("Sequential")]
public class KeycloakApiAuthTests : IClassFixture<KeycloakFixture>, IAsyncLifetime
{
    private readonly KeycloakFixture _keycloak;
    private KeycloakWebApplicationFactory? _factory;
    private HttpClient? _client;

    public KeycloakApiAuthTests(KeycloakFixture keycloak)
    {
        _keycloak = keycloak;
    }

    public async ValueTask InitializeAsync()
    {
        _factory = new KeycloakWebApplicationFactory(_keycloak.BaseUrl);
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null) _client.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns401_WithoutToken()
    {
        var response = await _client!.GetAsync("/api/routes", CancellationToken.None);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns200_WithValidToken()
    {
        var token = await _keycloak.GetTokenAsync("registered1", "password");
        token.ShouldNotBeNull();

        _client!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var response = await _client.GetAsync("/api/routes", CancellationToken.None);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns401_WithInvalidToken()
    {
        _client!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token-value");

        var response = await _client.GetAsync("/api/routes", CancellationToken.None);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
