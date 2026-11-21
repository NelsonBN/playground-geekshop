using GeekShop.Api.Catalogs;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products;
using GeekShop.Api.Products.DeleteProduct;

namespace GeekShop.Api.Tests.Products.DeleteProduct;

public sealed class DeleteProductHandlerTests
{
    private readonly IProductsRepository _repository = Substitute.For<IProductsRepository>();
    private readonly ICatalogsRepository _catalogs = Substitute.For<ICatalogsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ExistingProductInNoCatalog_When_Handle_Then_DeletesAndReturnsDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None).Returns([]);
        _repository.DeleteAsync(id, CancellationToken.None).Returns(true);
        var handler = new DeleteProductHandler(_repository, _catalogs, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.Deleted);
        await _repository.Received(1).DeleteAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task Given_ExistingProductInNoCatalog_When_Handle_Then_PublishesProductDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None).Returns([]);
        _repository.DeleteAsync(id, CancellationToken.None).Returns(true);
        var handler = new DeleteProductHandler(_repository, _catalogs, _publisher);

        // Act
        await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is ProductDeleted
                && ((ProductDeleted)e).Id == id
                && e.Topic == Topics.PRODUCTS),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_ProductAddedToACatalogMeanwhile_When_Handle_Then_ReturnsInCatalogWithoutPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None).Returns([]);
        _repository.DeleteAsync(id, CancellationToken.None).Returns(false);
        var handler = new DeleteProductHandler(_repository, _catalogs, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.InCatalog);
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProductInCatalog_When_Handle_Then_ReturnsInCatalogWithoutDeletingOrPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        _catalogs.ListByProductAsync(id, CancellationToken.None)
            .Returns([Catalog.Create("Summer", null)]);
        var handler = new DeleteProductHandler(_repository, _catalogs, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.InCatalog);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsNotFoundWithoutDeletingOrPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(false);
        var handler = new DeleteProductHandler(_repository, _catalogs, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().Be(DeleteProductResult.NotFound);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
