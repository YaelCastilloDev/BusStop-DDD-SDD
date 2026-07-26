using System.Net.Http.Headers;
using BusStop.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace BusStop.FunctionalTests.Auth;

public class KeycloakWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly string _keycloakBaseUrl;

    public KeycloakWebApplicationFactory(string keycloakBaseUrl)
    {
        _keycloakBaseUrl = keycloakBaseUrl;
        _dbContainer = new PostgreSqlBuilder("postgis/postgis:15-3.3")
            .WithDatabase("busstop_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var host = builder.Build();
        host.Start();

        using var scope = host.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();
        var logger = scopedServices.GetRequiredService<ILogger<KeycloakWebApplicationFactory>>();

        try
        {
            db.Database.EnsureCreated();
            Task.Run(() => SeedData.InitializeAsync(db)).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the database. Error: {exceptionMessage}", ex.Message);
            throw;
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var keycloakRealmUrl = $"{_keycloakBaseUrl}/realms/auth-demo";
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:PostgresConnection", _dbContainer.GetConnectionString() },
                { "Authentication:MetadataAddress", $"{keycloakRealmUrl}/.well-known/openid-configuration" },
                { "Authentication:ValidIssuer", keycloakRealmUrl },
                { "Authentication:Audience", "busstop-api" },
            });
        });
    }
}
