using System.Net;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Products.GetProduct;
using GeekShop.Api.Products.UpdateProduct;

namespace GeekShop.Api.IntegrationTests.Products.UpdateProduct;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class UpdateProductEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_PutProduct_Then_ReturnsNoContentAndPersistsChanges()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var update = new UpdateProductRequest { Name = $"Updated-{id}", Quantity = 1, Price = 2 };

        // Act
        var response = await client.PutAsJsonAsync($"/products/{id}", update, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await client.GetFromJsonAsync<GetProductResponse>(
            $"/products/{id}", TestContext.Current.CancellationToken);
        stored.Should().BeEquivalentTo(new GetProductResponse(id, update.Name, 1, 2, []));
    }

    [Fact]
    public async Task Given_UnknownId_When_PutProduct_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var update = new UpdateProductRequest { Name = "Updated", Quantity = 1, Price = 2 };

        // Act
        var response = await client.PutAsJsonAsync(
            $"/products/{Guid.NewGuid()}", update, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
