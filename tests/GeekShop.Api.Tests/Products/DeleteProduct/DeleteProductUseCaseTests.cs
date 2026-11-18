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
        _repository.ExistsAsync(id, TestContext.Current.CancellationToken).Returns(true);
        _catalogs.ListByProductAsync(id, TestContext.Current.CancellationToken).Returns([]);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(DeleteProductResult.Deleted);
        await _repository.Received(1).DeleteAsync(id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_Handle_Then_ReturnsInCatalogWithoutDeleting()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, TestContext.Current.CancellationToken).Returns(true);
        _catalogs.ListByProductAsync(id, TestContext.Current.CancellationToken)
            .Returns([new Catalog { Id = Guid.NewGuid(), Name = "Summer" }]);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(DeleteProductResult.InCatalog);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsNotFoundWithoutDeleting()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, TestContext.Current.CancellationToken).Returns(false);
        var useCase = new DeleteProductUseCase(_repository, _catalogs);

        // Act
        var result = await useCase.HandleAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(DeleteProductResult.NotFound);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
