using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.RemoveProductFromCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class RemoveProductFromCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
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
            $"/catalogs/{id}/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", TestContext.Current.CancellationToken);
        catalog!.Products.Should().BeEmpty();
        (await client.GetAsync($"/products/{productId}", TestContext.Current.CancellationToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
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
            $"/catalogs/{id}/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Given_UnknownProduct_When_RemoveProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());

        // Act
        var response = await client.DeleteAsync(
            $"/catalogs/{id}/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
