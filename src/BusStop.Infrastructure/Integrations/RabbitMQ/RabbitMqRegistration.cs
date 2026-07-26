using MassTransit;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

public static class RabbitMqRegistration
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        ConfigurationManager config,
        params System.Reflection.Assembly[] consumerAssemblies)
    {
        var settings = config.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {
            // Register consumers from the provided assemblies
            if (consumerAssemblies != null && consumerAssemblies.Length > 0)
            {
                x.AddConsumers(consumerAssemblies);
            }
            else
            {
                // Fallback to UseCases assembly
                x.AddConsumers(typeof(BusStop.UseCases.Users.Register.RegisterUserCommand).Assembly);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = config.GetConnectionString("messaging");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // If running in Aspire, use the injected connection string
                    cfg.Host(connectionString);
                }
                else
                {
                    // Fallback to manual settings
                    cfg.Host(settings.Host, settings.VirtualHost, h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });
                }

                cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(2)));

                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(1);
                });

                cfg.UseDelayedRedelivery(r =>
                    r.Intervals(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15)));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
