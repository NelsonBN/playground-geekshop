using GeekShop.Api.Messaging;
using GeekShop.Api.Products;
using GeekShop.Api.Products.AddProduct;

namespace GeekShop.Api.Tests.Products.AddProduct;

public sealed class AddProductHandlerTests
{
    private readonly IProductsRepository _repository = Substitute.For<IProductsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_StoresProductAndReturnsItsId()
    {
        // Arrange
        var handler = new AddProductHandler(_repository, _publisher);
        var request = new AddProductRequest { Name = "Keyboard", Quantity = 5, Price = 19.9 };

        // Act
        var id = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Id == id && p.Name == "Keyboard" && p.Quantity == 5 && p.Price == 19.9),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_PublishesProductAdded()
    {
        // Arrange
        var handler = new AddProductHandler(_repository, _publisher);
        var request = new AddProductRequest { Name = "Keyboard", Quantity = 5, Price = 19.9 };

        // Act
        var id = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is ProductAdded
                && ((ProductAdded)e).Id == id
                && ((ProductAdded)e).Name == "Keyboard"
                && ((ProductAdded)e).Quantity == 5
                && ((ProductAdded)e).Price == 19.9
                && e.Topic == Topics.PRODUCTS),
            CancellationToken.None);
    }
}
