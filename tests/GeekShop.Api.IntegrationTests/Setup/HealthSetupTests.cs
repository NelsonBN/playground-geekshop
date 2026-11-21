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
    public async Task Given_UnreachableRabbitMq_When_GetStartupOrReady_Then_ReturnsServiceUnavailable(string path)
    {
        // Arrange
        await using var factory = new ApiFactory(database.ConnectionString, UNREACHABLE_BROKER, exchangesWarmUp: false);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(path, CancellationToken.None);

        // Assert
        response.Should().Be503ServiceUnavailable();
    }

    [Fact]
    public async Task Given_UnreachableRabbitMq_When_GetLive_Then_StillReturnsHealthy()
    {
        // Arrange
        await using var factory = new ApiFactory(database.ConnectionString, UNREACHABLE_BROKER, exchangesWarmUp: false);
        var client = factory.CreateClient();

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
