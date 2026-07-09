using Ardalis.SharedKernel;
using BusStop.Infrastructure;

namespace BusStop.Web.Configurations;

public static class MediatorConfig
{
  public static IServiceCollection AddMediatorSourceGen(this IServiceCollection services,
    Microsoft.Extensions.Logging.ILogger logger)
  {
    logger.LogInformation("Registering Mediator SourceGen and Behaviors");
    services.AddMediator(options =>
    {
      options.ServiceLifetime = ServiceLifetime.Scoped;

      options.Assemblies =
      [
        typeof(BusStop.Core.RouteAggregate.Route), // Core
        typeof(InfrastructureServiceExtensions), // Infrastructure
        typeof(MediatorConfig),                  // Web
        typeof(BusStop.UseCases.Users.Register.RegisterUserCommand) // UseCases
      ];

      options.PipelineBehaviors =
      [
        typeof(DomainExceptionBehavior<,>),
        typeof(LoggingBehavior<,>),
        typeof(CurrentUserBehavior<,>)
      ];
    });

    return services;
  }
}
