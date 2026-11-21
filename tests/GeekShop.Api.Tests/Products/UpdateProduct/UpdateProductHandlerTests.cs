using GeekShop.Api.Messaging;
using GeekShop.Api.Products;
using GeekShop.Api.Products.UpdateProduct;

namespace GeekShop.Api.Tests.Products.UpdateProduct;

public sealed class UpdateProductHandlerTests
{
    private readonly IProductsRepository _repository = Substitute.For<IProductsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_UpdatesFieldsAndReturnsTrue()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Old", Quantity = 1, Price = 1 };
        _repository.GetAsync(product.Id, CancellationToken.None).Returns(product);
        var handler = new UpdateProductHandler(_repository, _publisher);
        var request = new UpdateProductRequest { Name = "New", Quantity = 7, Price = 3.5 };

        // Act
        var result = await handler.HandleAsync(product.Id, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        product.Should().BeEquivalentTo(new { product.Id, Name = "New", Quantity = 7u, Price = 3.5 });
        await _repository.Received(1).UpdateAsync(product, CancellationToken.None);
    }

    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_PublishesProductUpdated()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Old", Quantity = 1, Price = 1 };
        _repository.GetAsync(product.Id, CancellationToken.None).Returns(product);
        var handler = new UpdateProductHandler(_repository, _publisher);
        var request = new UpdateProductRequest { Name = "New", Quantity = 7, Price = 3.5 };

        // Act
        await handler.HandleAsync(product.Id, request, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is ProductUpdated
                && ((ProductUpdated)e).Id == product.Id
                && ((ProductUpdated)e).Name == "New"
                && ((ProductUpdated)e).Quantity == 7
                && ((ProductUpdated)e).Price == 3.5
                && e.Topic == Topics.PRODUCTS),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsFalseWithoutUpdatingOrPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetAsync(id, CancellationToken.None).Returns((Product?)null);
        var handler = new UpdateProductHandler(_repository, _publisher);
        var request = new UpdateProductRequest { Name = "New", Quantity = 7, Price = 3.5 };

        // Act
        var result = await handler.HandleAsync(id, request, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
