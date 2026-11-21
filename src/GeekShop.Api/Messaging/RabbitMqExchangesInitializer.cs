using RabbitMQ.Client;

namespace GeekShop.Api.Messaging;

/// <summary>Warm-up: makes sure every exchange exists before the first event is published.</summary>
public sealed class RabbitMqExchangesInitializer(IRabbitMqConnection connection) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var broker = await connection.GetAsync(cancellationToken);
        await using var channel = await broker.CreateChannelAsync(cancellationToken: cancellationToken);

        foreach(var topic in Topics.All)
        {
            await channel.ExchangeDeclareAsync(topic, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
