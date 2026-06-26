using BusStop.Infrastructure;

namespace BusStop.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructureServices(builder.Configuration, logger, typeof(ServiceConfigs).Assembly)
            .AddMediatorSourceGen(logger);

    services.AddSignalR();

    logger.LogInformation("{Project} services registered", "Mediator Source Generator and Infrastructure");

    return services;
  }


}
