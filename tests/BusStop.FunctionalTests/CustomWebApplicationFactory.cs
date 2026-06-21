using BusStop.Infrastructure.Data;

namespace BusStop.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  public ValueTask InitializeAsync() => ValueTask.CompletedTask;

  public new ValueTask DisposeAsync() => ValueTask.CompletedTask;

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
  }
}
