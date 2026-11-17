using GeekShop.Api.Products;
using GeekShop.Api.Products.GetProduct;

namespace GeekShop.Api.Tests.Products.GetProduct;

public sealed class GetProductUseCaseTests
{
    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_ReturnsMappedResponse()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Quantity = 5, Price = 19.9 };
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(product.Id, TestContext.Current.CancellationToken).Returns(product);
        var useCase = new GetProductUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(product.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(new GetProductResponse(product.Id, "Keyboard", 5, 19.9));
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsNull()
    {
        // Arrange
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var useCase = new GetProductUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
