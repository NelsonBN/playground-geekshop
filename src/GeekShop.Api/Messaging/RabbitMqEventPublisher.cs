using System.Text.Json;
using RabbitMQ.Client;

namespace GeekShop.Api.Messaging;

public sealed class RabbitMqEventPublisher(IRabbitMqConnection connection) : IEventPublisher
{
    private static readonly CreateChannelOptions _confirmed = new(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, integrationEvent.GetType(), JsonSerializerOptions.Web);

        var properties = new BasicProperties
        {
            MessageId = integrationEvent.EventId.ToString(),
            Type = integrationEvent.Subject,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        var broker = await connection.GetAsync(cancellationToken);
        await using var channel = await broker.CreateChannelAsync(_confirmed, cancellationToken);

        await channel.BasicPublishAsync(integrationEvent.Topic, integrationEvent.Subject, mandatory: false, properties, body, cancellationToken);
    }
}
