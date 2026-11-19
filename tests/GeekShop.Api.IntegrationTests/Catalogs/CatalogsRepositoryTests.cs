using GeekShop.Api.Catalogs;
using GeekShop.Api.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GeekShop.Api.IntegrationTests.Catalogs;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class CatalogsRepositoryTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ProductInCatalog_When_DeleteProductRejected_Then_MembershipIsKept()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var catalogId = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, catalogId, productId);
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICatalogsRepository>();

        // Act
        await client.DeleteAsync($"/products/{productId}", CancellationToken.None);

        // Assert
        (await repository.ListProductsAsync(catalogId, CancellationToken.None))
            .Should().ContainSingle().Which.Id.Should().Be(productId);
    }

    [Fact]
    public async Task Given_ProductInTwoCatalogs_When_ListByProduct_Then_ReturnsBothCatalogs()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productId = await ProductsApi.CreateAsync(client, ProductsApi.NewAddRequest());
        var first = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        var second = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        await CatalogsApi.AddProductAsync(client, first, productId);
        await CatalogsApi.AddProductAsync(client, second, productId);
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICatalogsRepository>();

        // Act
        var catalogs = await repository.ListByProductAsync(productId, CancellationToken.None);

        // Assert
        catalogs.Select(c => c.Id).Should().BeEquivalentTo([first, second]);
    }
}
