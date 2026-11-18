using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.AddProductToCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class AddProductToCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
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
            $"/catalogs/{id}/products/{productId}", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var catalog = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", TestContext.Current.CancellationToken);
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
            $"/catalogs/{id}/products/{Guid.NewGuid()}", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_UnknownCatalog_When_AddProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());

        // Act
        var response = await client.PutAsync(
            $"/catalogs/{Guid.NewGuid()}/products/{productId}", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
