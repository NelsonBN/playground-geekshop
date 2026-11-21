using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Setup;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class HealthSetupTests(DatabaseFixture database, RabbitMqFixture rabbitMq)
{
    // Nothing listens on port 1, so the connection is refused right away instead of waiting for a timeout.
    private const string UNREACHABLE_DATABASE =
        "Server=localhost,1;Database=GeekShop;User Id=sa;Password=unused;TrustServerCertificate=True;Connect Timeout=3";

    // The broker is unreachable on purpose here: an outage is a failure condition, not a stand-in for the broker.
    private const string UNREACHABLE_BROKER = "amqp://guest:guest@localhost:1";

    // Starts the production host against a healthy broker (its own virtual host), then takes the broker away.
    private async Task<(ApiFactory Factory, HttpClient Client)> _startThenLoseBrokerAsync()
    {
        var virtualHost = await rabbitMq.CreateVirtualHostAsync();
        var factory = new ApiFactory(database.ConnectionString, virtualHost);
        var client = factory.CreateClient();
        await rabbitMq.DeleteVirtualHostAsync(virtualHost);

        return (factory, client);
    }

    [Theory]
    [InlineData("/healthz/startup")]
    [InlineData("/healthz/live")]
    [InlineData("/healthz/ready")]
    public async Task Given_RunningSqlServerAndRabbitMq_When_GetProbe_Then_ReturnsHealthy(string path)
    {
        // Arrange
        await using var factory = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(path, CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
    }

    [Theory]
    [InlineData("/healthz/startup")]
    [InlineData("/healthz/ready")]
    public async Task Given_UnreachableSqlServer_When_GetStartupOrReady_Then_ReturnsServiceUnavailable(string path)
    {
        // Arrange
        await using var factory = new ApiFactory(UNREACHABLE_DATABASE, rabbitMq.ConnectionString);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(path, CancellationToken.None);

        // Assert
        response.Should().Be503ServiceUnavailable();
    }

    [Theory]
    [InlineData("/healthz/startup")]
    [InlineData("/healthz/ready")]
    public async Task Given_RabbitMqLostAfterStart_When_GetStartupOrReady_Then_ReturnsServiceUnavailable(string path)
    {
        // Arrange
        var (factory, client) = await _startThenLoseBrokerAsync();
        await using var _ = factory;

        // Act
        var response = await client.GetAsync(path, CancellationToken.None);

        // Assert
        response.Should().Be503ServiceUnavailable();
    }

    [Fact]
    public async Task Given_UnreachableRabbitMq_When_ApiStarts_Then_StartupFails()
    {
        // Arrange
        await using var factory = new ApiFactory(database.ConnectionString, UNREACHABLE_BROKER);

        // Act
        var start = () => factory.CreateClient();

        // Assert
        start.Should().Throw<Exception>();
    }

    [Fact]
    public async Task Given_RabbitMqLostAfterStart_When_GetLive_Then_StillReturnsHealthy()
    {
        // Arrange
        var (factory, client) = await _startThenLoseBrokerAsync();
        await using var _ = factory;

        // Act
        var response = await client.GetAsync("/healthz/live", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
    }

    [Fact]
    public async Task Given_UnreachableSqlServer_When_GetLive_Then_StillReturnsHealthy()
    {
        // Arrange
        await using var factory = new ApiFactory(UNREACHABLE_DATABASE, rabbitMq.ConnectionString);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz/live", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
    }
}
