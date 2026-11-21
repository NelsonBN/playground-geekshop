using GeekShop.Api.Messaging;
using GeekShop.Api.Products;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GeekShop.Api.Setup;

/// <summary>
/// Three probes, each answering a different question:
/// <c>startup</c> and <c>ready</c> check the real dependencies (SQL Server and RabbitMQ), while <c>live</c> only proves the process
/// answers, so a database outage never makes an orchestrator restart a healthy process.
/// </summary>
public static class HealthSetup
{
    private const string STARTUP_TAG = "startup";
    private const string LIVE_TAG = "live";
    private const string READY_TAG = "ready";

    public static IServiceCollection AddHealth(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LIVE_TAG])
            .AddDbContextCheck<ProductsDbContext>("sqlserver", tags: [STARTUP_TAG, READY_TAG])
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: [STARTUP_TAG, READY_TAG]);

        return services;
    }

    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
    {
        _mapProbe(endpoints, "/healthz/startup", STARTUP_TAG);
        _mapProbe(endpoints, "/healthz/live", LIVE_TAG);
        _mapProbe(endpoints, "/healthz/ready", READY_TAG);

        return endpoints;
    }

    private static void _mapProbe(IEndpointRouteBuilder endpoints, string path, string tag)
        => endpoints.MapHealthChecks(path, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(tag)
        });
}
