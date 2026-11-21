using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Messaging;

/// <summary>The API declares its exchanges when it starts: the broker is started empty, without a definitions file.</summary>
[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class RabbitMqExchangesInitializerTests(DatabaseFixture database, RabbitMqFixture rabbitMq)
{
    [Fact]
    public async Task Given_EmptyBroker_When_ApiStarts_Then_DeclaresEveryExchange()
    {
        // Arrange
        await using var factory = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString);

        // Act
        _ = factory.CreateClient();

        // Assert
        (await rabbitMq.ExistingExchangesAsync(Topics.All)).Should().BeEquivalentTo(Topics.All);
    }

    [Fact]
    public async Task Given_ExchangesAlreadyDeclared_When_ApiStartsAgain_Then_ServesRequestsAndKeepsEveryExchange()
    {
        // Arrange
        await using var first = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString);
        _ = first.CreateClient();
        await using var second = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString);

        // Act
        var response = await second.CreateClient().GetAsync("/products", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
        (await rabbitMq.ExistingExchangesAsync(Topics.All)).Should().BeEquivalentTo(Topics.All);
    }
}
