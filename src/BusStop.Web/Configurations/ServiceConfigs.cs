using BusStop.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace BusStop.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    services.AddCors(options =>
    {
      options.AddDefaultPolicy(policy =>
      {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
      });
    });

    services.AddInfrastructureServices(builder.Configuration, logger, typeof(ServiceConfigs).Assembly)
            .AddMediatorSourceGen(logger);

    services.AddSignalR();

    services.AddRateLimiter(options =>
    {
      options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
          RateLimitPartition.GetFixedWindowLimiter(
              partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
              factory: partition => new FixedWindowRateLimiterOptions
              {
                  AutoReplenishment = true,
                  PermitLimit = 100,
                  QueueLimit = 0,
                  Window = TimeSpan.FromMinutes(1)
              }));
      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    logger.LogInformation("{Project} services registered", "Mediator Source Generator and Infrastructure");

    return services;
  }


}
