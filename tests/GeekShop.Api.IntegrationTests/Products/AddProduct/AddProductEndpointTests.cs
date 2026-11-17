using System.Net;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Products.AddProduct;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class AddProductEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ValidRequest_When_PostProduct_Then_ReturnsCreatedWithLocation()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/products", ProductsApi.NewAddRequest(), TestContext.Current.CancellationToken);

        // Assert
        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.AbsolutePath.Should().Be($"/products/{id}");
    }

    [Fact]
    public async Task Given_ProductCreatedByAnotherHost_When_GetProduct_Then_DataSurvivedTheRestart()
    {
        // Arrange
        var request = ProductsApi.NewAddRequest();
        Guid id;
        await using(var first = new ApiFactory(database.ConnectionString))
        {
            id = await ProductsApi.CreateAsync(first.CreateClient(), request);
        }

        await using var second = new ApiFactory(database.ConnectionString);

        // Act
        var response = await second.CreateClient().GetAsync(
            $"/products/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
