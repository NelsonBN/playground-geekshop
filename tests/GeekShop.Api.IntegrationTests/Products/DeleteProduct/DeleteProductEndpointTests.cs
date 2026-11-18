using System.Net;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Products.DeleteProduct;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class DeleteProductEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_DeleteProduct_Then_ReturnsNoContentAndRemovesIt()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());

        // Act
        var response = await client.DeleteAsync($"/products/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var after = await client.GetAsync($"/products/{id}", TestContext.Current.CancellationToken);
        after.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var response = await client.DeleteAsync($"/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.GetAsync($"/products/{productId}", TestContext.Current.CancellationToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
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
            $"/catalogs/{catalogId}/products/{productId}", TestContext.Current.CancellationToken);

        // Act
        var response = await client.DeleteAsync($"/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Given_UnknownId_When_DeleteProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
