using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products.AddProduct;

namespace GeekShop.Api.IntegrationTests.Products.AddProduct;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class AddProductEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ValidRequest_When_PostProduct_Then_ReturnsCreatedWithLocation()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/products", ProductsApi.NewAddRequest(), CancellationToken.None);

        // Assert
        var id = await response.Content.ReadFromJsonAsync<Guid>(CancellationToken.None);
        response.Should().Be201Created();
        response.Headers.Location!.AbsolutePath.Should().Be($"/products/{id}");
    }

    [Fact]
    public async Task Given_ProductCreatedByAnotherHost_When_GetProduct_Then_DataSurvivedTheRestart()
    {
        // Arrange
        var request = ProductsApi.NewAddRequest();
        Guid id;
        await using(var first = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString))
        {
            id = await ProductsApi.CreateAsync(first.CreateClient(), request);
        }

        await using var second = new ApiFactory(database.ConnectionString, rabbitMq.ConnectionString);

        // Act
        var response = await second.CreateClient().GetAsync(
            $"/products/{id}", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
    }

    [Fact]
    public async Task Given_ValidRequest_When_PostProduct_Then_PublishesProductAdded()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = ProductsApi.NewAddRequest();

        // Act
        var id = await ProductsApi.CreateAsync(client, request);

        // Assert
        var received = await rabbitMq.ExpectAsync<ProductAdded>(Topics.PRODUCTS, id);
        received.Should().BeEquivalentTo(
            new ProductAdded(id, request.Name, request.Quantity, request.Price), BrokerEvents.IgnoreEnvelope);
    }
}
