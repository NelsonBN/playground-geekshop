using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GeekShop.Api.Messaging;

/// <summary>Opens a channel on the broker without sending anything, which proves connectivity.</summary>
public sealed class RabbitMqHealthCheck(IRabbitMqConnection connection) : IHealthCheck
{
    // Connecting can take long (the client retries); a probe must answer well before the orchestrator gives up.
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_timeout);

        try
        {
            var broker = await connection.GetAsync(timeout.Token);
            await using var channel = await broker.CreateChannelAsync(cancellationToken: timeout.Token);

            return HealthCheckResult.Healthy();
        }
        catch(Exception exception)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ is unreachable.", exception);
        }
    }
}
