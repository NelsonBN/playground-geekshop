using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.GetCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class GetCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CatalogWithProducts_When_GetCatalog_Then_ReturnsOkWithItsProducts()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productRequest = ProductsApi.NewAddRequest();
        var productId = await ProductsApi.CreateAsync(client, productRequest);
        var catalogRequest = CatalogsApi.NewAddRequest();
        var id = await CatalogsApi.CreateAsync(client, catalogRequest);
        await CatalogsApi.AddProductAsync(client, id, productId);

        // Act
        var response = await client.GetAsync($"/catalogs/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetCatalogResponse>(
            TestContext.Current.CancellationToken);
        body.Should().BeEquivalentTo(new GetCatalogResponse(
            id, catalogRequest.Name, catalogRequest.Description,
            [new CatalogProductResponse(productId, productRequest.Name)]));
    }

    [Fact]
    public async Task Given_UnknownId_When_GetCatalog_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/catalogs/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
