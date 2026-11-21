using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.DeleteCatalog;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.Tests.Catalogs.DeleteCatalog;

public sealed class DeleteCatalogHandlerTests
{
    private readonly ICatalogsRepository _repository = Substitute.For<ICatalogsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_DeletesAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        var handler = new DeleteCatalogHandler(_repository, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_PublishesCatalogDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(true);
        var handler = new DeleteCatalogHandler(_repository, _publisher);

        // Act
        await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is CatalogDeleted
                && ((CatalogDeleted)e).Id == id
                && e.Topic == Topics.CATALOGS),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutDeletingOrPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.ExistsAsync(id, CancellationToken.None).Returns(false);
        var handler = new DeleteCatalogHandler(_repository, _publisher);

        // Act
        var result = await handler.HandleAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
