using GeekShop.Api.Products;
using GeekShop.Api.Products.DeleteProduct;

namespace GeekShop.Api.Tests.Products.DeleteProduct;

public sealed class DeleteProductUseCaseTests
{
    [Fact]
    public async Task Given_ExistingProduct_When_Handle_Then_DeletesAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = Substitute.For<IProductsRepository>();
        repository.ExistsAsync(id, TestContext.Current.CancellationToken).Returns(true);
        var useCase = new DeleteProductUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
        await repository.Received(1).DeleteAsync(id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Given_MissingProduct_When_Handle_Then_ReturnsFalseWithoutDeleting()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = Substitute.For<IProductsRepository>();
        repository.ExistsAsync(id, TestContext.Current.CancellationToken).Returns(false);
        var useCase = new DeleteProductUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
        await repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
