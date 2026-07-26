using Testcontainers.RabbitMq;

namespace BusStop.IntegrationTests.Infrastructure.Messaging;

public sealed class RabbitMqFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Container = new RabbitMqBuilder("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        await Container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public string GetConnectionString() => Container.GetConnectionString();
}

[CollectionDefinition("RabbitMQ")]
public sealed class RabbitMqCollection : ICollectionFixture<RabbitMqFixture>
{
}
