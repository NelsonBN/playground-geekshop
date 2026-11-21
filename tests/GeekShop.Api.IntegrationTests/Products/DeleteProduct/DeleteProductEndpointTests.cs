using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products.DeleteProduct;

namespace GeekShop.Api.IntegrationTests.Products.DeleteProduct;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class DeleteProductEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_DeleteProduct_Then_ReturnsNoContentAndRemovesIt()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());

        // Act
        var response = await client.DeleteAsync($"/products/{id}", CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        var after = await client.GetAsync($"/products/{id}", CancellationToken.None);
        after.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_DeleteProduct_Then_ReturnsConflictAndKeepsIt()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, catalogId, productId);

        // Act
        var response = await client.DeleteAsync($"/products/{productId}", CancellationToken.None);

        // Assert
        response.Should().Be409Conflict();
        (await client.GetAsync($"/products/{productId}", CancellationToken.None))
            .Should().Be200Ok();
    }

    [Fact]
    public async Task Given_ProductRemovedFromCatalog_When_DeleteProduct_Then_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, catalogId, productId);
        await client.DeleteAsync(
            $"/catalogs/{catalogId}/products/{productId}", CancellationToken.None);

        // Act
        var response = await client.DeleteAsync($"/products/{productId}", CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
    }

    [Fact]
    public async Task Given_UnknownId_When_DeleteProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/products/{Guid.NewGuid()}", CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ExistingProduct_When_DeleteProduct_Then_PublishesProductDeleted()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());

        // Act
        await client.DeleteAsync($"/products/{id}", CancellationToken.None);

        // Assert
        var received = await rabbitMq.ExpectAsync<ProductDeleted>(Topics.PRODUCTS, id);
        received.Should().BeEquivalentTo(new ProductDeleted(id), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_DeleteProduct_Then_PublishesNothingForTheDeletion()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, catalogId, productId);

        // Act
        await client.DeleteAsync($"/products/{productId}", CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.PRODUCTS, nameof(ProductDeleted), productId);
    }

    [Fact]
    public async Task Given_UnknownProduct_When_DeleteProduct_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();

        // Act
        await client.DeleteAsync($"/products/{unknown}", CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.PRODUCTS, nameof(ProductDeleted), unknown);
    }
}
