using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>Hosts the real API in-process against the shared SQL Server container.</summary>
public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseSetting("ConnectionStrings:GeekShop", connectionString);
}
