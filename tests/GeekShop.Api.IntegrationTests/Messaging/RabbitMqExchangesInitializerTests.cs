using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Messaging;

/// <summary>
/// The API declares its exchanges when it starts. Each test runs against its own empty virtual host, so exchanges that
/// other tests' hosts declared on the shared broker cannot make a test pass by accident.
/// </summary>
[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class RabbitMqExchangesInitializerTests(DatabaseFixture database, RabbitMqFixture rabbitMq)
{
    [Fact]
    public async Task Given_EmptyBroker_When_ApiStarts_Then_DeclaresEveryExchange()
    {
        // Arrange
        var virtualHost = await rabbitMq.CreateVirtualHostAsync();
        var before = await rabbitMq.ExistingExchangesAsync(Topics.All, virtualHost);
        await using var factory = new ApiFactory(database.ConnectionString, virtualHost);

        // Act
        _ = factory.CreateClient();

        // Assert
        before.Should().BeEmpty();
        (await rabbitMq.ExistingExchangesAsync(Topics.All, virtualHost)).Should().BeEquivalentTo(Topics.All);
    }

    [Fact]
    public async Task Given_ExchangesAlreadyDeclared_When_ApiStartsAgain_Then_ServesRequestsAndKeepsEveryExchange()
    {
        // Arrange
        var virtualHost = await rabbitMq.CreateVirtualHostAsync();
        await using var first = new ApiFactory(database.ConnectionString, virtualHost);
        _ = first.CreateClient();
        await using var second = new ApiFactory(database.ConnectionString, virtualHost);

        // Act
        var response = await second.CreateClient().GetAsync("/products", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
        (await rabbitMq.ExistingExchangesAsync(Topics.All, virtualHost)).Should().BeEquivalentTo(Topics.All);
    }
}
