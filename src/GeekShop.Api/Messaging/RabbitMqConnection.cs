using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GeekShop.Api.Messaging;

public sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options) : IRabbitMqConnection, IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if(_connection is not { IsOpen: true })
            {
                if(_connection is not null)
                {
                    await _connection.DisposeAsync();
                }

                var factory = new ConnectionFactory { Uri = new Uri(options.Value.ConnectionString) };
                _connection = await factory.CreateConnectionAsync(cancellationToken);
            }

            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if(_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
