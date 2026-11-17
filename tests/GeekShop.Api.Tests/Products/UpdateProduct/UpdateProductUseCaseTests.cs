using GeekShop.Api.Products;
using GeekShop.Api.Products.UpdateProduct;

namespace GeekShop.Api.Tests.Products.UpdateProduct;

public sealed class UpdateProductUseCaseTests
{
    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_UpdatesFieldsAndReturnsTrue()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Old", Quantity = 1, Price = 1 };
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(product.Id, TestContext.Current.CancellationToken).Returns(product);
        var useCase = new UpdateProductUseCase(repository);
        var request = new UpdateProductRequest { Name = "New", Quantity = 7, Price = 3.5 };

        // Act
        var result = await useCase.HandleAsync(product.Id, request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
        product.Should().BeEquivalentTo(new { product.Id, Name = "New", Quantity = 7u, Price = 3.5 });
        await repository.Received(1).UpdateAsync(product, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsFalseWithoutUpdating()
    {
        // Arrange
        var repository = Substitute.For<IProductsRepository>();
        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var useCase = new UpdateProductUseCase(repository);
        var request = new UpdateProductRequest { Name = "New", Quantity = 7, Price = 3.5 };

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
