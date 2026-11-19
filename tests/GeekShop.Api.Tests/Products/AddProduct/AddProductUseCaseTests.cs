using GeekShop.Api.Products;
using GeekShop.Api.Products.AddProduct;

namespace GeekShop.Api.Tests.Products.AddProduct;

public sealed class AddProductUseCaseTests
{
    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_StoresProductAndReturnsItsId()
    {
        // Arrange
        var repository = Substitute.For<IProductsRepository>();
        var useCase = new AddProductUseCase(repository);
        var request = new AddProductRequest { Name = "Keyboard", Quantity = 5, Price = 19.9 };

        // Act
        var id = await useCase.HandleAsync(request, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        await repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Id == id && p.Name == "Keyboard" && p.Quantity == 5 && p.Price == 19.9),
            CancellationToken.None);
    }
}
