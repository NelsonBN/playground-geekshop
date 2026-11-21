using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Products;

namespace GeekShop.Api.Tests.Catalogs.GetCatalog;

public sealed class GetCatalogHandlerTests
{
    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_ReturnsDetailsWithProducts()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", "Deals");
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Quantity = 5, Price = 19.9 };
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(catalog.Id, CancellationToken.None).Returns(catalog);
        repository.ListProductsAsync(catalog.Id, CancellationToken.None).Returns([product]);
        var handler = new GetCatalogHandler(repository);

        // Act
        var result = await handler.HandleAsync(catalog.Id, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new GetCatalogResponse(
            catalog.Id, "Summer", "Deals", [new CatalogProductResponse(product.Id, "Keyboard")]));
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsNull()
    {
        // Arrange
        var repository = Substitute.For<ICatalogsRepository>();
        var id = Guid.NewGuid();
        repository.GetAsync(id, CancellationToken.None).Returns((Catalog?)null);
        var handler = new GetCatalogHandler(repository);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await repository.DidNotReceive().ListProductsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
