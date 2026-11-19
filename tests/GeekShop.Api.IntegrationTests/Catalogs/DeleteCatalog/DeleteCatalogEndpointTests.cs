using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.DeleteCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class DeleteCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
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
}
