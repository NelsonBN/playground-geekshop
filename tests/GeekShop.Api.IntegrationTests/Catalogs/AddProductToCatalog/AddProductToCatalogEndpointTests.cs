using System.Net;
using GeekShop.Api.Catalogs.AddProductToCatalog;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Catalogs.AddProductToCatalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class AddProductToCatalogEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ProductAlreadyInCatalog_When_AddProductAgain_Then_ReturnsNoContentWithSingleEntry()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, id, productId);

        // Act
        var response = await client.PutAsync(
            $"/catalogs/{id}/products/{productId}", null, CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        catalog!.Products.Should().ContainSingle().Which.Id.Should().Be(productId);
    }

    [Fact]
    public async Task Given_ConcurrentRequests_When_AddSameProduct_Then_AllReturnNoContentWithSingleEntry()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.PutAsync(
            $"/catalogs/{id}/products/{productId}", null, CancellationToken.None)));

        // Assert
        responses.Select(r => r.StatusCode).Should().AllBeEquivalentTo(HttpStatusCode.NoContent);
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        catalog!.Products.Should().ContainSingle().Which.Id.Should().Be(productId);
    }

    [Fact]
    public async Task Given_UnknownProduct_When_AddProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        var response = await client.PutAsync(
            $"/catalogs/{id}/products/{Guid.NewGuid()}", null, CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_UnknownCatalog_When_AddProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());

        // Act
        var response = await client.PutAsync(
            $"/catalogs/{Guid.NewGuid()}/products/{productId}", null, CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ExistingCatalogAndProduct_When_PutCatalogProduct_Then_PublishesProductAddedToCatalog()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        await CatalogsApi.AddProductAsync(client, catalogId, productId);

        // Assert
        var received = await rabbitMq.ExpectAsync<ProductAddedToCatalog>(Topics.CATALOGS, catalogId, "catalogId");
        received.Should().BeEquivalentTo(new ProductAddedToCatalog(catalogId, productId), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_UnknownIds_When_PutCatalogProduct_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();

        // Act
        await client.PutAsync($"/catalogs/{unknown}/products/{unknown}", null, CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.CATALOGS, nameof(ProductAddedToCatalog), unknown, "catalogId");
    }
}
