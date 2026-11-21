using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products.GetProduct;
using GeekShop.Api.Products.UpdateProduct;

namespace GeekShop.Api.IntegrationTests.Products.UpdateProduct;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class UpdateProductEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedProduct_When_PutProduct_Then_ReturnsNoContentAndPersistsChanges()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var update = new UpdateProductRequest { Name = $"Updated-{id}", Quantity = 1, Price = 2 };

        // Act
        var response = await client.PutAsJsonAsync($"/products/{id}", update, CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        var stored = await client.GetFromJsonAsync<GetProductResponse>(
            $"/products/{id}", CancellationToken.None);
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
            $"/products/{Guid.NewGuid()}", update, CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ExistingProduct_When_PutProduct_Then_PublishesProductUpdated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var request = new UpdateProductRequest { Name = "Renamed", Quantity = 9, Price = 4.5 };

        // Act
        await client.PutAsJsonAsync($"/products/{id}", request, CancellationToken.None);

        // Assert
        var received = await rabbitMq.ExpectAsync<ProductUpdated>(Topics.PRODUCTS, id);
        received.Should().BeEquivalentTo(new ProductUpdated(id, "Renamed", 9, 4.5), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_UnknownProduct_When_PutProduct_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();
        var request = new UpdateProductRequest { Name = "x", Quantity = 1, Price = 1 };

        // Act
        await client.PutAsJsonAsync($"/products/{unknown}", request, CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.PRODUCTS, nameof(ProductUpdated), unknown);
    }
}
