using GeekShop.Api.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace GeekShop.Api.Tests.Messaging;

public sealed class RabbitMqHealthCheckTests
{
    private readonly IRabbitMqConnection _connection = Substitute.For<IRabbitMqConnection>();
    private readonly IConnection _broker = Substitute.For<IConnection>();

    [Fact]
    public async Task Given_ReachableBroker_When_Check_Then_IsHealthy()
    {
        // Arrange
        _connection.GetAsync(Arg.Any<CancellationToken>()).Returns(_broker);
        _broker.CreateChannelAsync(Arg.Any<CreateChannelOptions?>(), Arg.Any<CancellationToken>()).Returns(Substitute.For<IChannel>());
        var check = new RabbitMqHealthCheck(_connection);

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Given_UnreachableBroker_When_Check_Then_IsUnhealthyWithTheCause()
    {
        // Arrange
        var failure = new InvalidOperationException("unreachable");
        _connection.GetAsync(Arg.Any<CancellationToken>()).Returns<IConnection>(_ => throw failure);
        var check = new RabbitMqHealthCheck(_connection);

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().BeSameAs(failure);
    }
}
