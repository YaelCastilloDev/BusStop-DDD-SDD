using BusStop.Core.Interfaces;
using BusStop.Core.Services;
using BusStop.Infrastructure.Data;
using BusStop.Infrastructure.Data.Queries;
using BusStop.UseCases.Contributors.List;

namespace BusStop.Infrastructure;
public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    // Try to get connection strings in order of priority:
    // 1. "busstop" - provided by Aspire when using .WithReference(busStopDb)
    // 2. "DefaultConnection" - SQL Server (Windows only by default, can be forced with USE_SQL_SERVER=true)
    // 3. "SqliteConnection" - fallback to SQLite
    bool isWindows = OperatingSystem.IsWindows();
    bool forceSqlServer = Environment.GetEnvironmentVariable("USE_SQL_SERVER") == "true";
    
    string? connectionString = config.GetConnectionString("busstop")
                               ?? ((isWindows || forceSqlServer) ? config.GetConnectionString("DefaultConnection") : null)
                               ?? config.GetConnectionString("SqliteConnection");
    Guard.Against.Null(connectionString);

    services.AddScoped<EventDispatchInterceptor>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<AppDbContext>((provider, options) =>
    {
      var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
      
      // Use SQL Server if Aspire or DefaultConnection (on Windows or forced) is available, otherwise use SQLite
      if (config.GetConnectionString("busstop") != null || 
          ((isWindows || forceSqlServer) && config.GetConnectionString("DefaultConnection") != null))
      {
        options.UseSqlServer(connectionString);
      }
      else
      {
        options.UseSqlite(connectionString);
      }
      
      options.AddInterceptors(eventDispatchInterceptor);
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
           .AddScoped<IDeleteContributorService, DeleteContributorService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
