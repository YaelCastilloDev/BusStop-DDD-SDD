using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using BusStop.Infrastructure.Integrations.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BusStop.UnitTests.Integrations.Keycloak;

public class KeycloakAdminServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static IConfiguration CreateConfiguration(
        string baseUrl = "http://localhost:8080",
        string realm = "test-realm",
        string clientId = "test-client",
        string clientSecret = "test-secret")
    {
        var config = Substitute.For<IConfiguration>();
        config["Keycloak:Admin:BaseUrl"].Returns(baseUrl);
        config["Keycloak:Admin:Realm"].Returns(realm);
        config["Keycloak:Admin:ClientId"].Returns(clientId);
        config["Keycloak:Admin:ClientSecret"].Returns(clientSecret);
        return config;
    }

    private static (KeycloakAdminService service, TestHttpMessageHandler handler) CreateService(
        HttpStatusCode tokenStatus = HttpStatusCode.OK,
        string? tokenBody = null,
        HttpStatusCode createStatus = HttpStatusCode.Created,
        string? createBody = null,
        string? baseUrl = null)
    {
        var handler = new TestHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient("KeycloakAdmin").Returns(httpClient);

        var config = CreateConfiguration(baseUrl: baseUrl ?? "http://localhost:8080");
        var logger = Substitute.For<ILogger<KeycloakAdminService>>();

        var service = new KeycloakAdminService(httpClientFactory, config, logger);

        // Queue token response
        var resolvedTokenBody = tokenBody ?? GetValidTokenResponse();
        handler.EnqueueResponse(new HttpResponseMessage(tokenStatus)
        {
            Content = new StringContent(resolvedTokenBody)
        });

        // Queue user creation response (only used when token succeeds)
        if (tokenStatus == HttpStatusCode.OK && resolvedTokenBody.Contains("access_token"))
        {
            handler.EnqueueResponse(new HttpResponseMessage(createStatus)
            {
                Content = new StringContent(createBody ?? string.Empty)
            });
        }

        return (service, handler);
    }

    private static string GetValidTokenResponse()
    {
        return JsonSerializer.Serialize(new { access_token = "test-admin-token", token_type = "Bearer", expires_in = 300 }, JsonOptions);
    }

    private static string GetTokenResponseWithoutAccessToken()
    {
        return JsonSerializer.Serialize(new { access_token = "" }, JsonOptions);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsSuccess_WhenTokenAcquisitionAndUserCreationSucceed()
    {
        // Arrange
        var (service, handler) = CreateService();

        // Act
        var result = await service.CreateUserAsync("test@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        handler.Requests.Count.ShouldBe(2);

        // Verify token request
        var tokenRequest = handler.Requests[0];
        tokenRequest.RequestUri!.AbsoluteUri.ShouldContain("/realms/test-realm/protocol/openid-connect/token");
        tokenRequest.Method.ShouldBe(HttpMethod.Post);
        var tokenBody = await tokenRequest.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
        tokenBody.ShouldContain("grant_type=client_credentials");
        tokenBody.ShouldContain("client_id=test-client");
        tokenBody.ShouldContain("client_secret=test-secret");

        // Verify user creation request has Bearer token
        var createRequest = handler.Requests[1];
        createRequest.RequestUri!.AbsoluteUri.ShouldContain("/admin/realms/test-realm/users");
        createRequest.Method.ShouldBe(HttpMethod.Post);
        createRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        createRequest.Headers.Authorization!.Parameter.ShouldBe("test-admin-token");
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsError_WhenTokenRequestFails()
    {
        // Arrange
        var (service, _) = CreateService(
            tokenStatus: HttpStatusCode.BadRequest,
            tokenBody: "{\"error\":\"invalid_client\"}");

        // Act
        var result = await service.CreateUserAsync("test@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Failed to authenticate with the identity provider. Please check the server configuration.");
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsError_WhenTokenResponseMissingAccessToken()
    {
        // Arrange
        // When the token response has an empty access_token, the inner method returns
        // an error, which the outer method wraps as an authentication failure.
        var (service, _) = CreateService(
            tokenBody: GetTokenResponseWithoutAccessToken());

        // Act
        var result = await service.CreateUserAsync("test@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Failed to authenticate with the identity provider. Please check the server configuration.");
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsSuccess_WhenUserCreated()
    {
        // Arrange
        var (service, _) = CreateService(
            createStatus: HttpStatusCode.Created);

        // Act
        var result = await service.CreateUserAsync("test@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsError_WhenUserAlreadyExists()
    {
        // Arrange
        var (service, _) = CreateService(
            createStatus: HttpStatusCode.Conflict,
            createBody: "{\"error\":\"User exists\"}");

        // Act
        var result = await service.CreateUserAsync("existing@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("A user with this email already exists.");
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsError_WhenUserCreationFailsWithServerError()
    {
        // Arrange
        var (service, _) = CreateService(
            createStatus: HttpStatusCode.InternalServerError,
            createBody: "Internal Server Error");

        // Act
        var result = await service.CreateUserAsync("test@example.com", "Password123!", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Failed to create user account. The authentication service returned an unexpected error.");
    }

    /// <summary>
    /// Test double for <see cref="HttpMessageHandler"/> that queues responses
    /// and records all requests for later inspection.
    /// </summary>
    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = new();

        public void EnqueueResponse(HttpResponseMessage response)
        {
            _responses.Enqueue(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (_responses.Count == 0)
                throw new InvalidOperationException("No response queued for this request.");
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
