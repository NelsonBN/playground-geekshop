using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Catalogs.RemoveProductFromCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Catalogs.RemoveProductFromCatalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class RemoveProductFromCatalogEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ProductInCatalog_When_RemoveProduct_Then_ReturnsNoContentAndKeepsProduct()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, id, productId);

        // Act
        var response = await client.DeleteAsync(
            $"/catalogs/{id}/products/{productId}", CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        catalog!.Products.Should().BeEmpty();
        (await client.GetAsync($"/products/{productId}", CancellationToken.None))
            .Should().Be200Ok();
    }

    [Fact]
    public async Task Given_ProductNotInCatalog_When_RemoveProduct_Then_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        var response = await client.DeleteAsync(
            $"/catalogs/{id}/products/{productId}", CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
    }

    [Fact]
    public async Task Given_ConcurrentRequests_When_RemoveSameProduct_Then_AllReturnNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, id, productId);

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.DeleteAsync(
            $"/catalogs/{id}/products/{productId}", CancellationToken.None)));

        // Assert
        responses.Select(r => r.StatusCode).Should().AllBeEquivalentTo(HttpStatusCode.NoContent);
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        catalog!.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_UnknownProduct_When_RemoveProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        var response = await client.DeleteAsync(
            $"/catalogs/{id}/products/{Guid.NewGuid()}", CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_DeleteCatalogProduct_Then_PublishesProductRemovedFromCatalog()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, catalogId, productId);

        // Act
        await client.DeleteAsync($"/catalogs/{catalogId}/products/{productId}", CancellationToken.None);

        // Assert
        var received = await rabbitMq.ExpectAsync<ProductRemovedFromCatalog>(Topics.CATALOGS, catalogId, "catalogId");
        received.Should().BeEquivalentTo(
            new ProductRemovedFromCatalog(catalogId, productId), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_UnknownIds_When_DeleteCatalogProduct_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();

        // Act
        await client.DeleteAsync($"/catalogs/{unknown}/products/{unknown}", CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.CATALOGS, nameof(ProductRemovedFromCatalog), unknown, "catalogId");
    }
}
