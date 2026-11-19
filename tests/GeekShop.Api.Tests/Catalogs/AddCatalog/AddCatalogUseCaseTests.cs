using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.AddCatalog;

namespace GeekShop.Api.Tests.Catalogs.AddCatalog;

public sealed class AddCatalogUseCaseTests
{
    [Fact]
    public async Task Given_Request_When_Handle_Then_PersistsCatalogAndReturnsItsId()
    {
        // Arrange
        var repository = Substitute.For<ICatalogsRepository>();
        var useCase = new AddCatalogUseCase(repository);
        var request = new AddCatalogRequest { Name = "Summer", Description = "Summer deals" };

        // Act
        var id = await useCase.HandleAsync(request, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        await repository.Received(1).AddAsync(
            Arg.Is<Catalog>(c => c.Id == id && c.Name == "Summer" && c.Description == "Summer deals"),
            CancellationToken.None);
    }
}
