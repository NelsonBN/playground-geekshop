using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Products.GetProduct;

namespace GeekShop.Api.IntegrationTests.Products.GetProduct;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class GetProductEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_GetProduct_Then_ReturnsOkWithPersistedProduct()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = ProductsApi.NewAddRequest();
        var id = await ProductsApi.CreateAsync(client, request);

        // Act
        var response = await client.GetAsync($"/products/{id}", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
        var body = await response.Content.ReadFromJsonAsync<GetProductResponse>(
            CancellationToken.None);
        body.Should().BeEquivalentTo(new GetProductResponse(id, request.Name, 5, 19.9, []));
    }

    [Fact]
    public async Task Given_ProductInCatalogs_When_GetProduct_Then_ReturnsTheCatalogsItBelongsTo()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogRequest = CatalogsApi.NewAddRequest();
        var catalogId = await CatalogsApi.CreateAsync(client, catalogRequest);
        await CatalogsApi.AddProductAsync(client, catalogId, productId);

        // Act
        var body = await client.GetFromJsonAsync<GetProductResponse>(
            $"/products/{productId}", CancellationToken.None);

        // Assert
        body!.Catalogs.Should().BeEquivalentTo([new ProductCatalogResponse(catalogId, catalogRequest.Name)]);
    }

    [Fact]
    public async Task Given_UnknownId_When_GetProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/products/{Guid.NewGuid()}", CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }
}
