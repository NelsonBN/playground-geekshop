using GeekShop.Api.Catalogs;
using GeekShop.Api.Products;
using GeekShop.Api.Products.DeleteProduct;

namespace GeekShop.Api.Tests.Products.DeleteProduct;

public sealed class DeleteProductUseCaseTests
{
    private readonly IProductsRepository _repository = Substitute.For<IProductsRepository>();
    private readonly ICatalogsRepository _catalogs = Substitute.For<ICatalogsRepository>();

    [Fact]
    public async Task Given_ExistingProductInNoCatalog_When_Handle_Then_DeletesAndReturnsDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None).Returns([]);
        _repository.DeleteAsync(id, CancellationToken.None).Returns(true);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.Deleted);
        await _repository.Received(1).DeleteAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task Given_ProductAddedToACatalogMeanwhile_When_Handle_Then_ReturnsInCatalog()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None).Returns([]);
        _repository.DeleteAsync(id, CancellationToken.None).Returns(false);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.InCatalog);
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_Handle_Then_ReturnsInCatalogWithoutDeleting()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None)
            .Returns([Catalog.Create("Summer", null)]);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.InCatalog);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsNotFoundWithoutDeleting()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(false);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.NotFound);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
