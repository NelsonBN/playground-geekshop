using System.Text.Json;
using GeekShop.Api.Messaging;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>
/// One disposable RabbitMQ broker shared by every integration test class in the collection. It starts empty: the API
/// declares the exchanges when it starts, and the fixture only adds the queue the tests read from.
/// </summary>
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private const string QUEUE = "tests";
    private const string IMAGE = "rabbitmq:4.4.0-management";

    private readonly RabbitMqContainer _container = new RabbitMqBuilder(IMAGE).Build();

    private IConnection? _connection;
    private IChannel? _channel;

    /// <summary>The AMQP URI every test host publishes to.</summary>
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connection = await new ConnectionFactory { Uri = new Uri(ConnectionString) }.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task DisposeAsync()
    {
        if(_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if(_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    /// <summary>
    /// Binds the queue the tests read from to every exchange. The exchanges must already exist, because the API declares
    /// them when it starts, so call this after a host started. Publishing before the binding exists is lost.
    /// </summary>
    public async Task EnsureSubscriptionsAsync()
    {
        await _channel!.QueueDeclareAsync(QUEUE, durable: true, exclusive: false, autoDelete: false);

        foreach(var topic in Topics.All)
        {
            await _channel.QueueBindAsync(QUEUE, topic, routingKey: "#");
        }
    }

    /// <summary>The exchanges, out of those given, that exist on the broker.</summary>
    public async Task<IReadOnlyList<string>> ExistingExchangesAsync(IEnumerable<string> exchanges)
    {
        var existing = new List<string>();
        foreach(var exchange in exchanges)
        {
            if(await _exchangeExistsAsync(exchange))
            {
                existing.Add(exchange);
            }
        }

        return existing;
    }

    // A passive declare fails when the exchange does not exist.
    private async Task<bool> _exchangeExistsAsync(string exchange)
    {
        try
        {
            await using var channel = await _connection!.CreateChannelAsync();
            await channel.ExchangeDeclarePassiveAsync(exchange);

            return true;
        }
        catch(RabbitMQ.Client.Exceptions.OperationInterruptedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for an event that matches, reading the queue bound to every exchange. Other tests' events share the
    /// broker, so everything that does not match is skipped (and consumed, which is fine: collections run one test at a time).
    /// </summary>
    public async Task<ReceivedEvent?> ReceiveAsync(string topic, Func<ReceivedEvent, bool> match, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);

        try
        {
            while(!cancellation.IsCancellationRequested)
            {
                var message = await _channel!.BasicGetAsync(QUEUE, autoAck: true, cancellation.Token);
                if(message is null)
                {
                    await Task.Delay(100, cancellation.Token);
                    continue;
                }

                var received = new ReceivedEvent(
                    message.Exchange,
                    message.BasicProperties.MessageId ?? string.Empty,
                    message.BasicProperties.Type ?? string.Empty,
                    message.BasicProperties.ContentType,
                    JsonDocument.Parse(message.Body).RootElement.Clone());

                if(received.Topic == topic && match(received))
                {
                    return received;
                }
            }
        }
        catch(OperationCanceledException)
        {
            // Timed out without a match.
        }

        return null;
    }
}

/// <summary>An event as it arrived from the broker.</summary>
public sealed record ReceivedEvent(string Topic, string MessageId, string Subject, string? ContentType, JsonElement Body)
{
    public bool HasId(Guid id, string property = "id")
        => Body.TryGetProperty(property, out var value) && value.TryGetGuid(out var actual) && actual == id;
}
