using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BusStop.IntegrationTests.Infrastructure.Messaging;

[Collection("RabbitMQ")]
public class MassTransitIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqFixture _fixture;
    private ServiceProvider? _provider;
    private IBusControl? _bus;

    public MassTransitIntegrationTests(RabbitMqFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TestMessageConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(_fixture.GetConnectionString()), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("test-queue", e =>
                {
                    e.ConfigureConsumer<TestMessageConsumer>(context);
                });
            });
        });

        _provider = services.BuildServiceProvider();
        _bus = _provider.GetRequiredService<IBusControl>();
        await _bus.StartAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        if (_bus is not null)
        {
            await _bus.StopAsync(CancellationToken.None);
        }
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }
    }

    [Fact]
    public async Task MassTransit_PublishAndConsume_RoundTripSucceeds()
    {
        var message = new TestMessage { Content = "Hello RabbitMQ" };
        TestMessageConsumer.ReceivedMessages.Clear();

        await _bus!.Publish(message, CancellationToken.None);

        await Task.Delay(2000, CancellationToken.None);

        TestMessageConsumer.ReceivedMessages.ShouldNotBeEmpty();
        TestMessageConsumer.ReceivedMessages.ShouldContain(m => m.Content == "Hello RabbitMQ");
    }

    [Fact]
    public async Task MassTransit_Bus_StartsAndConnects()
    {
        var busReady = _bus!.GetType().GetProperty("Address")?.GetValue(_bus);
        busReady.ShouldNotBeNull();
    }

    [Fact]
    public async Task MassTransit_Publish_MultipleMessagesSucceed()
    {
        TestMessageConsumer.ReceivedMessages.Clear();

        for (int i = 0; i < 5; i++)
        {
            await _bus!.Publish(new TestMessage { Content = $"Message-{i}" }, CancellationToken.None);
        }

        await Task.Delay(3000, CancellationToken.None);

        TestMessageConsumer.ReceivedMessages.Count.ShouldBe(5);
    }
}

public record TestMessage
{
    public string Content { get; init; } = string.Empty;
}

public sealed class TestMessageConsumer : IConsumer<TestMessage>
{
    public static List<TestMessage> ReceivedMessages { get; } = new();

    public Task Consume(ConsumeContext<TestMessage> context)
    {
        ReceivedMessages.Add(context.Message);
        return Task.CompletedTask;
    }
}
