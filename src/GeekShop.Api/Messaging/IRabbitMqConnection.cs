using RabbitMQ.Client;

namespace GeekShop.Api.Messaging;

/// <summary>The one connection to the broker, opened on first use so the API can start while the broker is down.</summary>
public interface IRabbitMqConnection
{
    Task<IConnection> GetAsync(CancellationToken cancellationToken);
}
