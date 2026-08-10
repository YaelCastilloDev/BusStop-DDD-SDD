using System.Net;
using System.Text.Json;
using BusStop.Infrastructure.Integrations.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BusStop.UnitTests.Integrations;

/// <summary>
/// Documents that <see cref="KeycloakAdminService"/> fetches a new admin token
/// on every call to <see cref="KeycloakAdminService.CreateUserAsync"/> instead of
/// caching it. Each invocation of CreateUserAsync results in a full token request
/// to Keycloak's token endpoint, even when called in rapid succession on the same
/// service instance. The expected behavior is that the token should be cached and
/// reused until it expires.
/// </summary>
public class KeycloakAdminServiceTokenCacheTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static IConfiguration CreateConfiguration()
    {
        var config = Substitute.For<IConfiguration>();
        config["Keycloak:Admin:BaseUrl"].Returns("http://localhost:8080");
        config["Keycloak:Admin:Realm"].Returns("test-realm");
        config["Keycloak:Admin:ClientId"].Returns("test-client");
        config["Keycloak:Admin:ClientSecret"].Returns("test-secret");
        return config;
    }

    private static string GetValidTokenResponse()
    {
        return JsonSerializer.Serialize(
            new { access_token = "test-admin-token", token_type = "Bearer", expires_in = 300 },
            JsonOptions);
    }

    /// <summary>
    /// Confirms the bug: calling <c>CreateUserAsync</c> twice on the same instance
    /// results in the token endpoint being hit twice. A properly cached token would
    /// result in only one token request.
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_FetchesTokenEveryCall_NoCachingOccurs()
    {
        // Arrange
        var handler = new TestHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient("KeycloakAdmin").Returns(httpClient);

        var config = CreateConfiguration();
        var logger = Substitute.For<ILogger<KeycloakAdminService>>();
        var service = new KeycloakAdminService(httpClientFactory, config, logger);

        // Queue responses for two full CreateUserAsync cycles.
        // Each cycle = 1 token request + 1 user creation request = 4 total requests.
        for (int i = 0; i < 2; i++)
        {
            handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetValidTokenResponse())
            });
            handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(string.Empty)
            });
        }

        // Act: Call CreateUserAsync twice on the same service instance.
        var result1 = await service.CreateUserAsync("user1@example.com", "Password1!", TestContext.Current.CancellationToken);
        var result2 = await service.CreateUserAsync("user2@example.com", "Password2!", TestContext.Current.CancellationToken);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        // 4 total HTTP requests were made (2 token requests + 2 user creation requests).
        handler.Requests.Count.ShouldBe(4);

        // Both token endpoint requests were made. This confirms the bug:
        // the token is NOT cached — it is fetched fresh on every CreateUserAsync call.
        // Expected fix: only 1 token request should be made; subsequent calls should
        // reuse the cached token.
        var tokenRequests = handler.Requests
            .Where(r => r.RequestUri!.AbsoluteUri.Contains("/protocol/openid-connect/token"))
            .ToList();
        tokenRequests.Count.ShouldBe(2);
        tokenRequests.ForEach(r => r.Method.ShouldBe(HttpMethod.Post));

        // Both user creation requests carry the Bearer token.
        var createRequests = handler.Requests
            .Where(r => r.RequestUri!.AbsoluteUri.Contains("/admin/realms/test-realm/users"))
            .ToList();
        createRequests.Count.ShouldBe(2);
        createRequests.ForEach(r =>
        {
            r.Headers.Authorization!.Scheme.ShouldBe("Bearer");
            r.Headers.Authorization!.Parameter.ShouldBe("test-admin-token");
        });
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
