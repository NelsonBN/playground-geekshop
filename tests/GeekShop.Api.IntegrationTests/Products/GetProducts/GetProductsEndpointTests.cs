using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Products.GetProducts;

namespace GeekShop.Api.IntegrationTests.Products.GetProducts;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class GetProductsEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_GetProducts_Then_ListContainsIt()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = ProductsApi.NewAddRequest();
        var id = await ProductsApi.CreateAsync(client, request);

        // Act
        var response = await client.GetAsync("/products", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
        var body = await response.Content.ReadFromJsonAsync<List<GetProductsResponse>>(
            CancellationToken.None);
        body.Should().Contain(new GetProductsResponse(id, request.Name));
    }
}
