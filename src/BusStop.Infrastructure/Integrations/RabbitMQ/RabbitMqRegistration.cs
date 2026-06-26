using MassTransit;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

public static class RabbitMqRegistration
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        ConfigurationManager config)
    {
        var settings = config.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
