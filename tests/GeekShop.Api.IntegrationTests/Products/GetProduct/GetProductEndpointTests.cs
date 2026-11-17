using System.Net;
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
        var response = await client.GetAsync($"/products/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetProductResponse>(
            TestContext.Current.CancellationToken);
        body.Should().Be(new GetProductResponse(id, request.Name, 5, 19.9));
    }

    [Fact]
    public async Task Given_UnknownId_When_GetProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
