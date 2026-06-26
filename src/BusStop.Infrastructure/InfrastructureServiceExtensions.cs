using BusStop.Infrastructure.Data;
using BusStop.Infrastructure.Integrations.RabbitMQ;

namespace BusStop.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    string? connectionString = config.GetConnectionString("PostgresConnection");
    Guard.Against.Null(connectionString);

    services.AddScoped<EventDispatchInterceptor>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<AppDbContext>((provider, options) =>
    {
      var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();
      options.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
      options.AddInterceptors(eventDispatchInterceptor);
    });

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

    services.AddScoped<BusStop.UseCases.Routes.GetNearby.INearbyRoutesQueryService, BusStop.Infrastructure.Data.Queries.NearbyRoutesQueryService>();
    services.AddRabbitMqMessaging(config);

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
