using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.UpdateCatalog;

namespace GeekShop.Api.Tests.Catalogs.UpdateCatalog;

public sealed class UpdateCatalogUseCaseTests
{
    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_UpdatesAndReturnsTrue()
    {
        // Arrange
        var catalog = Catalog.Create("Old", "Old description");
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(catalog.Id, CancellationToken.None).Returns(catalog);
        var useCase = new UpdateCatalogUseCase(repository);
        var request = new UpdateCatalogRequest { Name = "New", Description = "New description" };

        // Act
        var result = await useCase.HandleAsync(catalog.Id, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await repository.Received(1).UpdateAsync(
            Arg.Is<Catalog>(c => c.Id == catalog.Id && c.Name == "New" && c.Description == "New description"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutUpdating()
    {
        // Arrange
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Catalog?)null);
        var useCase = new UpdateCatalogUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(
            Guid.NewGuid(), new UpdateCatalogRequest { Name = "New" }, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Catalog>(), Arg.Any<CancellationToken>());
    }
}
