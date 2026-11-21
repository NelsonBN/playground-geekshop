using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.RemoveProductFromCatalog;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products;

namespace GeekShop.Api.Tests.Catalogs.RemoveProductFromCatalog;

public sealed class RemoveProductFromCatalogHandlerTests
{
    private readonly ICatalogsRepository _catalogs = Substitute.For<ICatalogsRepository>();
    private readonly IProductsRepository _products = Substitute.For<IProductsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ExistingCatalogAndProduct_When_Handle_Then_RemovesProductAndReturnsTrue()
    {
        // Arrange
        var catalogId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _catalogs.ExistsAsync(catalogId, CancellationToken.None).Returns(true);
        _products.ExistsAsync(productId, CancellationToken.None).Returns(true);
        var handler = new RemoveProductFromCatalogHandler(_catalogs, _products, _publisher);

        // Act
        var result = await handler.HandleAsync(catalogId, productId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _catalogs.Received(1).RemoveProductAsync(catalogId, productId, CancellationToken.None);
    }

    [Fact]
    public async Task Given_ExistingCatalogAndProduct_When_Handle_Then_PublishesProductRemovedFromCatalog()
    {
        // Arrange
        var catalogId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _catalogs.ExistsAsync(catalogId, CancellationToken.None).Returns(true);
        _products.ExistsAsync(productId, CancellationToken.None).Returns(true);
        var handler = new RemoveProductFromCatalogHandler(_catalogs, _products, _publisher);

        // Act
        await handler.HandleAsync(catalogId, productId, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is ProductRemovedFromCatalog
                && ((ProductRemovedFromCatalog)e).CatalogId == catalogId
                && ((ProductRemovedFromCatalog)e).ProductId == productId
                && e.Topic == Topics.CATALOGS),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutRemovingOrPublishing()
    {
        // Arrange
        var catalogId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _catalogs.ExistsAsync(catalogId, CancellationToken.None).Returns(false);
        _products.ExistsAsync(productId, CancellationToken.None).Returns(true);
        var handler = new RemoveProductFromCatalogHandler(_catalogs, _products, _publisher);

        // Act
        var result = await handler.HandleAsync(catalogId, productId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _catalogs.DidNotReceive().RemoveProductAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsFalseWithoutRemovingOrPublishing()
    {
        // Arrange
        var catalogId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _catalogs.ExistsAsync(catalogId, CancellationToken.None).Returns(true);
        _products.ExistsAsync(productId, CancellationToken.None).Returns(false);
        var handler = new RemoveProductFromCatalogHandler(_catalogs, _products, _publisher);

        // Act
        var result = await handler.HandleAsync(catalogId, productId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _catalogs.DidNotReceive().RemoveProductAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
