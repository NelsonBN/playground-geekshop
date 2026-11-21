using GeekShop.Api.Catalogs.DeleteCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Catalogs.DeleteCatalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class DeleteCatalogEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CatalogWithProduct_When_DeleteCatalog_Then_ReturnsNoContentAndKeepsProduct()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, id, productId);

        // Act
        var response = await client.DeleteAsync($"/catalogs/{id}", CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        (await client.GetAsync($"/catalogs/{id}", CancellationToken.None))
            .Should().Be404NotFound();
        (await client.GetAsync($"/products/{productId}", CancellationToken.None))
            .Should().Be200Ok();
    }

    [Fact]
    public async Task Given_UnknownId_When_DeleteCatalog_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/catalogs/{Guid.NewGuid()}", CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ExistingCatalog_When_DeleteCatalog_Then_PublishesCatalogDeleted()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        await client.DeleteAsync($"/catalogs/{id}", CancellationToken.None);

        // Assert
        var received = await rabbitMq.ExpectAsync<CatalogDeleted>(Topics.CATALOGS, id);
        received.Should().BeEquivalentTo(new CatalogDeleted(id), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_UnknownCatalog_When_DeleteCatalog_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();

        // Act
        await client.DeleteAsync($"/catalogs/{unknown}", CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.CATALOGS, nameof(CatalogDeleted), unknown);
    }
}
