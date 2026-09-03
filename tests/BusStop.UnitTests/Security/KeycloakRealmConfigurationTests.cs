using System.Text.Json;

namespace BusStop.UnitTests.Security;

public class KeycloakRealmConfigurationTests
{
    private static readonly JsonElement Realm = LoadRealm();

    [Fact]
    public void Realm_Enforces_IdentitySecurityControls()
    {
        Realm.GetProperty("sslRequired").GetString().ShouldBe("external");
        Realm.GetProperty("loginTheme").GetString().ShouldBe("busstop");
        Realm.GetProperty("verifyEmail").GetBoolean().ShouldBeTrue();
        Realm.GetProperty("bruteForceProtected").GetBoolean().ShouldBeTrue();
        (Realm.GetProperty("passwordPolicy").GetString() ?? string.Empty).ShouldContain("length(12)");
    }

    [Fact]
    public void BrowserClient_Uses_CodeFlowWithPkce_AndNoPrivilegedGrants()
    {
        var client = GetClient("busstop-web");

        client.GetProperty("publicClient").GetBoolean().ShouldBeTrue();
        client.GetProperty("standardFlowEnabled").GetBoolean().ShouldBeTrue();
        client.GetProperty("directAccessGrantsEnabled").GetBoolean().ShouldBeFalse();
        client.GetProperty("implicitFlowEnabled").GetBoolean().ShouldBeFalse();
        client.GetProperty("serviceAccountsEnabled").GetBoolean().ShouldBeFalse();
        client.TryGetProperty("secret", out _).ShouldBeFalse();
        client.GetProperty("attributes")
            .GetProperty("pkce.code.challenge.method")
            .GetString()
            .ShouldBe("S256");

        var audienceMapper = client.GetProperty("protocolMappers")
            .EnumerateArray()
            .Single(mapper => mapper.GetProperty("protocolMapper").GetString() == "oidc-audience-mapper");
        audienceMapper.GetProperty("config")
            .GetProperty("included.client.audience")
            .GetString()
            .ShouldBe("busstop-api");

        var rolesMapper = client.GetProperty("protocolMappers")
            .EnumerateArray()
            .Single(mapper => mapper.GetProperty("protocolMapper").GetString() == "oidc-usermodel-realm-role-mapper");
        rolesMapper.GetProperty("config")
            .GetProperty("claim.name")
            .GetString()
            .ShouldBe("realm_access.roles");
    }

    [Fact]
    public void ApiClient_IsBearerOnly_AndCannotIssueTokens()
    {
        var client = GetClient("busstop-api");

        client.GetProperty("bearerOnly").GetBoolean().ShouldBeTrue();
        client.GetProperty("standardFlowEnabled").GetBoolean().ShouldBeFalse();
        client.GetProperty("directAccessGrantsEnabled").GetBoolean().ShouldBeFalse();
        client.GetProperty("serviceAccountsEnabled").GetBoolean().ShouldBeFalse();
        client.TryGetProperty("secret", out _).ShouldBeFalse();

        var hasServiceAccountUser = Realm.GetProperty("users")
            .EnumerateArray()
            .Any(user => user.TryGetProperty("serviceAccountClientId", out _));
        hasServiceAccountUser.ShouldBeFalse();
    }

    [Fact]
    public void SwaggerBrowserClient_RequiresPkce()
    {
        var client = GetClient("busstop-swagger");

        client.GetProperty("publicClient").GetBoolean().ShouldBeTrue();
        client.GetProperty("directAccessGrantsEnabled").GetBoolean().ShouldBeFalse();
        client.GetProperty("attributes")
            .GetProperty("pkce.code.challenge.method")
            .GetString()
            .ShouldBe("S256");
    }

    private static JsonElement GetClient(string clientId)
    {
        return Realm.GetProperty("clients")
            .EnumerateArray()
            .Single(client => client.GetProperty("clientId").GetString() == clientId);
    }

    private static JsonElement LoadRealm()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "containers", "realm-export.json");
            if (File.Exists(path))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                return document.RootElement.Clone();
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate containers/realm-export.json.");
    }
}
