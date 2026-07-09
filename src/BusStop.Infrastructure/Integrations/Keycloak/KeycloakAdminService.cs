using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.Infrastructure.Integrations.Keycloak;

public class KeycloakAdminService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<KeycloakAdminService> logger) : IKeycloakAdminService
{
    public async Task<Result> CreateUserAsync(string email, string password, CancellationToken ct)
    {
        try
        {
            var baseUrl = configuration["Keycloak:Admin:BaseUrl"];
            var realm = configuration["Keycloak:Admin:Realm"];
            var clientId = configuration["Keycloak:Admin:ClientId"];
            var clientSecret = configuration["Keycloak:Admin:ClientSecret"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                return Result.Error("Keycloak Admin BaseUrl is required.");
            if (string.IsNullOrWhiteSpace(realm))
                return Result.Error("Keycloak Admin Realm is required.");
            if (string.IsNullOrWhiteSpace(clientId))
                return Result.Error("Keycloak Admin ClientId is required.");
            if (string.IsNullOrWhiteSpace(clientSecret))
                return Result.Error("Keycloak Admin ClientSecret is required.");

            var client = httpClientFactory.CreateClient("KeycloakAdmin");

            // Step 1: Obtain an admin access token
            var tokenResult = await GetAdminAccessTokenAsync(client, baseUrl, realm, clientId, clientSecret, ct);
            if (!tokenResult.IsSuccess)
                return Result.Error("Failed to authenticate with the identity provider. Please check the server configuration.");

            // Step 2: Create the user in the realm
            return await CreateUserInRealmAsync(client, baseUrl, realm, tokenResult.Value, email, password, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error creating Keycloak user for {Email}", email);
            return Result.Error("An unexpected error occurred while creating the user account.");
        }
    }

    private async Task<Result<string>> GetAdminAccessTokenAsync(
        HttpClient client,
        string baseUrl,
        string realm,
        string clientId,
        string clientSecret,
        CancellationToken ct)
    {
        var tokenEndpoint = $"{baseUrl.TrimEnd('/')}/realms/{realm}/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        logger.LogInformation("Requesting Keycloak admin token for realm {Realm}", realm);

        HttpResponseMessage tokenResponse;
        try
        {
            tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error requesting Keycloak admin token from {TokenEndpoint}", tokenEndpoint);
            return Result<string>.Error("Unable to connect to the authentication service. Please try again later.");
        }

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorBody = await tokenResponse.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "Keycloak admin token request failed. Status: {StatusCode}, Body: {ErrorBody}",
                (int)tokenResponse.StatusCode,
                errorBody);
            return Result<string>.Error("Authentication service returned an error. Please check the server configuration.");
        }

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
        var accessToken = tokenJson.GetProperty("access_token").GetString();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogError("Keycloak token response missing access_token field");
            return Result<string>.Error("Received an invalid response from the authentication service.");
        }

        logger.LogInformation("Successfully obtained Keycloak admin token");
        return Result<string>.Success(accessToken);
    }

    private async Task<Result> CreateUserInRealmAsync(
        HttpClient client,
        string baseUrl,
        string realm,
        string accessToken,
        string email,
        string password,
        CancellationToken ct)
    {
        var usersEndpoint = $"{baseUrl.TrimEnd('/')}/admin/realms/{realm}/users";

        var userPayload = new
        {
            email,
            username = email,
            enabled = true,
            emailVerified = false,
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            },
            realmRoles = new[] { "RegisteredUser" },
            requiredActions = new[] { "VERIFY_EMAIL" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, usersEndpoint)
        {
            Content = JsonContent.Create(userPayload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        logger.LogInformation("Creating Keycloak user in realm {Realm} with email {Email}", realm, email);

        HttpResponseMessage createResponse;
        try
        {
            createResponse = await client.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error creating Keycloak user for {Email}", email);
            return Result.Error("Unable to connect to the authentication service. Please try again later.");
        }

        if (createResponse.IsSuccessStatusCode)
        {
            logger.LogInformation("Successfully created Keycloak user: {Email}", email);
            return Result.Success();
        }

        var errorBody = await createResponse.Content.ReadAsStringAsync(ct);

        if (createResponse.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogWarning("Keycloak user already exists: {Email}", email);
            return Result.Error("A user with this email already exists.");
        }

        logger.LogError(
            "Keycloak user creation failed. Status: {StatusCode}, Body: {ErrorBody}",
            (int)createResponse.StatusCode,
            errorBody);
        return Result.Error("Failed to create user account. The authentication service returned an unexpected error.");
    }
}
