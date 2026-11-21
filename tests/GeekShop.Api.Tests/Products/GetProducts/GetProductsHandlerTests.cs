using GeekShop.Api.Products;
using GeekShop.Api.Products.GetProducts;

namespace GeekShop.Api.Tests.Products.GetProducts;

public sealed class GetProductsHandlerTests
{
    [Fact]
    public async Task Given_StoredProducts_When_Handle_Then_ReturnsIdAndNameOfEach()
    {
        // Arrange
        var first = new Product { Id = Guid.NewGuid(), Name = "A", Quantity = 1, Price = 1 };
        var second = new Product { Id = Guid.NewGuid(), Name = "B", Quantity = 2, Price = 2 };
        var repository = Substitute.For<IProductsRepository>();
        repository.ListAsync(CancellationToken.None).Returns([first, second]);
        var handler = new GetProductsHandler(repository);

        // Act
        var result = await handler.HandleAsync(CancellationToken.None);

        // Assert
        result.Should().Equal(
            new GetProductsResponse(first.Id, "A"),
            new GetProductsResponse(second.Id, "B"));
    }

    [Fact]
    public async Task Given_NoProducts_When_Handle_Then_ReturnsEmpty()
    {
        // Arrange
        var repository = Substitute.For<IProductsRepository>();
        repository.ListAsync(CancellationToken.None).Returns([]);
        var handler = new GetProductsHandler(repository);

        // Act
        var result = await handler.HandleAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
