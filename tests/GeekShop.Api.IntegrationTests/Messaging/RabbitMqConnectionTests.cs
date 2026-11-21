using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;

namespace GeekShop.Api.IntegrationTests.Messaging;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class RabbitMqConnectionTests(RabbitMqFixture rabbitMq)
{
    private static RabbitMqConnection _create(string connectionString)
        => new(Options.Create(new RabbitMqOptions { ConnectionString = connectionString }));

    [Fact]
    public async Task Given_ReachableBroker_When_Get_Then_ReturnsAnOpenConnection()
    {
        // Arrange
        await using var connection = _create(rabbitMq.ConnectionString);

        // Act
        var broker = await connection.GetAsync(CancellationToken.None);

        // Assert
        broker.IsOpen.Should().BeTrue();
    }

    [Fact]
    public async Task Given_OpenConnection_When_GetAgain_Then_ReusesIt()
    {
        // Arrange
        await using var connection = _create(rabbitMq.ConnectionString);
        var first = await connection.GetAsync(CancellationToken.None);

        // Act
        var second = await connection.GetAsync(CancellationToken.None);

        // Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task Given_ClosedConnection_When_Get_Then_OpensANewOne()
    {
        // Arrange
        await using var connection = _create(rabbitMq.ConnectionString);
        var first = await connection.GetAsync(CancellationToken.None);
        await first.CloseAsync(200, "test", TimeSpan.FromSeconds(5), false, CancellationToken.None);

        // Act
        var second = await connection.GetAsync(CancellationToken.None);

        // Assert
        second.Should().NotBeSameAs(first);
        second.IsOpen.Should().BeTrue();
    }

    [Fact]
    public async Task Given_UnreachableBroker_When_Get_Then_Throws()
    {
        // Arrange
        await using var connection = _create("amqp://guest:guest@localhost:1");

        // Act
        var get = () => connection.GetAsync(CancellationToken.None);

        // Assert
        await get.Should().ThrowAsync<BrokerUnreachableException>();
    }
}
