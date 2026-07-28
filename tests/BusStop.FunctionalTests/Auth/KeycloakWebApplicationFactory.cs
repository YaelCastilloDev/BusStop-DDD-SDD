using System.Net.Http.Headers;
using BusStop.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace BusStop.FunctionalTests.Auth;

public class KeycloakWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly string _keycloakBaseUrl;

    public KeycloakWebApplicationFactory(string keycloakBaseUrl)
    {
        _keycloakBaseUrl = keycloakBaseUrl;
        _dbContainer = new PostgreSqlBuilder("postgis/postgis:15-3.3")
            .WithDatabase("busstop_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgresConnection", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__messaging", _rabbitMqContainer.GetConnectionString());
        var keycloakRealmUrl = $"{_keycloakBaseUrl}/realms/auth-demo";
        Environment.SetEnvironmentVariable("Authentication__MetadataAddress", $"{keycloakRealmUrl}/.well-known/openid-configuration");
        Environment.SetEnvironmentVariable("Authentication__ValidIssuer", keycloakRealmUrl);
        Environment.SetEnvironmentVariable("Authentication__Audience", "busstop-api");
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
