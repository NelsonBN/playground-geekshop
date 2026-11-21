using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>
/// Hosts the real API in-process against the shared SQL Server and RabbitMQ containers. The host publishes to the real
/// broker and declares the exchanges when it starts, like in production. A host that must start while the broker is
/// unreachable skips that warm-up with <paramref name="exchangesWarmUp"/>.
/// </summary>
public sealed class ApiFactory(string connectionString, string rabbitMq, bool exchangesWarmUp = true) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:GeekShop", connectionString);
        builder.UseSetting("RabbitMq:ConnectionString", rabbitMq);

        if(!exchangesWarmUp)
        {
            builder.ConfigureTestServices(services => services.RemoveAll<IHostedService>());
        }
    }
}
