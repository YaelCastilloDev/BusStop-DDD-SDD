using BusStop.Infrastructure.Data;
using BusStop.Infrastructure.Integrations.RabbitMQ;
using Resend;

namespace BusStop.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger,
    params System.Reflection.Assembly[] additionalAssemblies)
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
    
    var resendApiKey = config["Resend:ApiKey"];
    if (string.IsNullOrWhiteSpace(resendApiKey))
    {
      logger.LogWarning("No Resend:ApiKey found in configuration. Email sending will fail if triggered.");
    }
    
    services.AddResend(options => options.ApiToken = resendApiKey ?? "missing-key");
    services.AddScoped<BusStop.Core.NotificationAggregate.Interfaces.IEmailSender, BusStop.Infrastructure.Integrations.Email.ResendEmailSender>();
    logger.LogInformation("Resend email sender registered.");
    
    var assemblies = new List<System.Reflection.Assembly> { typeof(BusStop.UseCases.Users.Register.RegisterUserCommand).Assembly };
    if (additionalAssemblies != null)
    {
      assemblies.AddRange(additionalAssemblies);
    }
    services.AddRabbitMqMessaging(config, assemblies.ToArray());

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
