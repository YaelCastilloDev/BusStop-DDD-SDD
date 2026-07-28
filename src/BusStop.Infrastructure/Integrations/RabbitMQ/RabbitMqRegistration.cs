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

                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(2)));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
