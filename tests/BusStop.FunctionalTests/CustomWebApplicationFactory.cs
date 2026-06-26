using BusStop.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace BusStop.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
      .WithImage("postgis/postgis:15-3.3")
      .WithDatabase("busstop_test")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

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
    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

    try
    {
      db.Database.EnsureCreated();
      SeedData.InitializeAsync(db).GetAwaiter().GetResult();
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
