using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace BusStop.FunctionalTests.Auth;

public sealed class KeycloakFixture : IAsyncLifetime
{
    private IContainer? _container;
    private readonly string _realmExportPath;
    private string? _testRealmExportPath;

    public const string TestClientId = "busstop-functional-tests";
    public const string TestClientSecret = "functional-tests-only";

    public string BaseUrl { get; private set; } = string.Empty;
    public string RealmUrl => $"{BaseUrl}/realms/auth-demo";
    public string TokenUrl => $"{RealmUrl}/protocol/openid-connect/token";

    public KeycloakFixture()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "containers", "realm-export.json");
            if (File.Exists(candidate)) { _realmExportPath = candidate; break; }
            dir = dir.Parent;
        }

        if (string.IsNullOrEmpty(_realmExportPath))
        {
            _realmExportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "..", "..", "containers", "realm-export.json"));
        }
    }

    public async ValueTask InitializeAsync()
    {
        _testRealmExportPath = CreateTestRealmExport();

        _container = new ContainerBuilder("quay.io/keycloak/keycloak:26.5.2")
            .WithPortBinding(8080, true)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("KC_HTTP_ENABLED", "true")
            .WithEnvironment("KC_HOSTNAME", "localhost")
            .WithBindMount(_testRealmExportPath, "/opt/keycloak/data/import/realm-export.json")
            .WithCommand("start-dev", "--import-realm")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/realms/auth-demo")))
            .Build();

        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(8080);
        BaseUrl = $"http://localhost:{mappedPort}";
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();

        if (_testRealmExportPath is not null && File.Exists(_testRealmExportPath))
            File.Delete(_testRealmExportPath);
    }

    public async Task<TokenResponse?> GetTokenAsync(string username, string password)
    {
        using var client = new HttpClient();
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{TestClientId}:{TestClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", TestClientId),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("audience", "busstop-api"),
        });

        var response = await client.PostAsync(TokenUrl, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    private string CreateTestRealmExport()
    {
        var realm = JsonNode.Parse(File.ReadAllText(_realmExportPath))?.AsObject()
            ?? throw new InvalidOperationException("The Keycloak realm export is invalid.");
        var clients = realm["clients"]?.AsArray()
            ?? throw new InvalidOperationException("The Keycloak realm export has no clients array.");

        clients.Add(JsonNode.Parse(
            $$"""
            {
              "clientId": "{{TestClientId}}",
              "name": "BusStop Functional Tests",
              "enabled": true,
              "protocol": "openid-connect",
              "publicClient": false,
              "secret": "{{TestClientSecret}}",
              "standardFlowEnabled": false,
              "implicitFlowEnabled": false,
              "directAccessGrantsEnabled": true,
              "serviceAccountsEnabled": false,
              "authorizationServicesEnabled": false,
              "bearerOnly": false,
              "consentRequired": false,
              "fullScopeAllowed": true,
              "protocolMappers": [
                {
                  "name": "busstop-api-audience",
                  "protocol": "openid-connect",
                  "protocolMapper": "oidc-audience-mapper",
                  "consentRequired": false,
                  "config": {
                    "included.client.audience": "busstop-api",
                    "id.token.claim": "false",
                    "access.token.claim": "true",
                    "introspection.token.claim": "true"
                  }
                }
              ]
            }
            """));

        var outputPath = Path.Combine(Path.GetTempPath(), $"busstop-keycloak-realm-{Guid.NewGuid():N}.json");
        File.WriteAllText(outputPath, realm.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return outputPath;
    }
}

public sealed record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }
}
