using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>
/// Hosts the real API in-process against the shared SQL Server and RabbitMQ containers. The host is the production one,
/// hosted services included: it publishes to the real broker and declares the exchanges when it starts, so it does not
/// start while the broker is unreachable, exactly like in production. Only the connection settings differ.
/// </summary>
public sealed class ApiFactory(string connectionString, string rabbitMq) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:GeekShop", connectionString);
        builder.UseSetting("RabbitMq:ConnectionString", rabbitMq);
    }
}
