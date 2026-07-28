using BusStop.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace BusStop.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgis/postgis:15-3.3")
      .WithDatabase("busstop_test")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

  private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3-management")
      .WithUsername("guest")
      .WithPassword("guest")
      .Build();

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
    var host = builder.Build();
    host.Start();

    using var scope = host.Services.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var db = scopedServices.GetRequiredService<AppDbContext>();
    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

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
          config.AddInMemoryCollection(new Dictionary<string, string?>
          {
              { "ConnectionStrings:PostgresConnection", _dbContainer.GetConnectionString() }
          });
      });
  }
}
