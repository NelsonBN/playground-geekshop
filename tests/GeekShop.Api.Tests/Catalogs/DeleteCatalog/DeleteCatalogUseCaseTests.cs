using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.DeleteCatalog;

namespace GeekShop.Api.Tests.Catalogs.DeleteCatalog;

public sealed class DeleteCatalogUseCaseTests
{
    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_DeletesAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = Substitute.For<ICatalogsRepository>();
        repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        var useCase = new DeleteCatalogUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await repository.Received(1).DeleteAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutDeleting()
    {
        // Arrange
        var repository = Substitute.For<ICatalogsRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new DeleteCatalogUseCase(repository);

        // Act
        var result = await useCase.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
