using GeekShop.Api.Catalogs;
using GeekShop.Api.Products;
using GeekShop.Api.Products.GetProduct;

namespace GeekShop.Api.Tests.Products.GetProduct;

public sealed class GetProductUseCaseTests
{
    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_ReturnsMappedResponseWithCatalogs()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Quantity = 5, Price = 19.9 };
        var catalog = Catalog.Create("Peripherals", null);
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(product.Id, CancellationToken.None).Returns(product);
        var catalogs = Substitute.For<ICatalogsRepository>();
        catalogs.ListByProductAsync(product.Id, CancellationToken.None).Returns([catalog]);
        var useCase = new GetProductUseCase(repository, catalogs);

        // Act
        var result = await useCase.HandleAsync(product.Id, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new GetProductResponse(
            product.Id, "Keyboard", 5, 19.9, [new ProductCatalogResponse(catalog.Id, "Peripherals")]));
    }

    [Fact]
    public async Task Given_ProductWithoutCatalogs_When_Handle_Then_ReturnsEmptyCatalogs()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Quantity = 5, Price = 19.9 };
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(product.Id, CancellationToken.None).Returns(product);
        var catalogs = Substitute.For<ICatalogsRepository>();
        catalogs.ListByProductAsync(product.Id, CancellationToken.None).Returns([]);
        var useCase = new GetProductUseCase(repository, catalogs);

        // Act
        var result = await useCase.HandleAsync(product.Id, CancellationToken.None);

        // Assert
        result!.Catalogs.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsNull()
    {
        // Arrange
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var catalogs = Substitute.For<ICatalogsRepository>();
        var useCase = new GetProductUseCase(repository, catalogs);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await catalogs.DidNotReceive().ListByProductAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
