using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.AddProductToCatalog;
using GeekShop.Api.Products;

namespace GeekShop.Api.Tests.Catalogs.AddProductToCatalog;

public sealed class AddProductToCatalogUseCaseTests
{
    private readonly ICatalogsRepository _catalogs = Substitute.For<ICatalogsRepository>();
    private readonly IProductsRepository _products = Substitute.For<IProductsRepository>();

    [Fact]
    public async Task Given_ExistingCatalogAndProduct_When_Handle_Then_AddsProductAndReturnsTrue()
    {
        // Arrange
        var catalogId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _catalogs.ExistsAsync(catalogId, CancellationToken.None).Returns(true);
        _products.ExistsAsync(productId, CancellationToken.None).Returns(true);
        var useCase = new AddProductToCatalogUseCase(_catalogs, _products);

        // Act
        var result = await useCase.HandleAsync(catalogId, productId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _catalogs.Received(1).AddProductAsync(catalogId, productId, CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutAdding()
    {
        // Arrange
        _catalogs.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _products.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var useCase = new AddProductToCatalogUseCase(_catalogs, _products);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _catalogs.DidNotReceive().AddProductAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsFalseWithoutAdding()
    {
        // Arrange
        _catalogs.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        _products.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new AddProductToCatalogUseCase(_catalogs, _products);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _catalogs.DidNotReceive().AddProductAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
