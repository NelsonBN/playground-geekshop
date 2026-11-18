using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.UpdateCatalog;

namespace GeekShop.Api.Tests.Catalogs.UpdateCatalog;

public sealed class UpdateCatalogUseCaseTests
{
    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_UpdatesAndReturnsTrue()
    {
        // Arrange
        var catalog = new Catalog { Id = Guid.NewGuid(), Name = "Old", Description = "Old description" };
        var repository = Substitute.For<ICatalogsRepository>();
        repository.GetAsync(catalog.Id, TestContext.Current.CancellationToken).Returns(catalog);
        var useCase = new UpdateCatalogUseCase(repository);
        var request = new UpdateCatalogRequest { Name = "New", Description = "New description" };

        // Act
        var result = await useCase.HandleAsync(catalog.Id, request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
        await repository.Received(1).UpdateAsync(
            Arg.Is<Catalog>(c => c.Id == catalog.Id && c.Name == "New" && c.Description == "New description"),
            TestContext.Current.CancellationToken);
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
            Guid.NewGuid(), new UpdateCatalogRequest { Name = "New" }, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Catalog>(), Arg.Any<CancellationToken>());
    }
}
