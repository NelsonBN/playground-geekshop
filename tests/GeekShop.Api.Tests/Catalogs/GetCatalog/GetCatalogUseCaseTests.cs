using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Products;

namespace GeekShop.Api.Tests.Catalogs.GetCatalog;

public sealed class GetCatalogUseCaseTests
{
    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_ReturnsDetailsWithProducts()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", "Deals");
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Quantity = 5, Price = 19.9 };
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(catalog.Id, TestContext.Current.CancellationToken).Returns(catalog);
        repository.ListProductsAsync(catalog.Id, TestContext.Current.CancellationToken).Returns([product]);
        var useCase = new GetCatalogUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(catalog.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new GetCatalogResponse(
            catalog.Id, "Summer", "Deals", [new CatalogProductResponse(product.Id, "Keyboard")]));
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsNull()
    {
        // Arrange
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Catalog?)null);
        var useCase = new GetCatalogUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
        await repository.DidNotReceive().ListProductsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
