using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.UpdateCatalog;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.Tests.Catalogs.UpdateCatalog;

public sealed class UpdateCatalogHandlerTests
{
    private readonly ICatalogsRepository _repository = Substitute.For<ICatalogsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_UpdatesAndReturnsTrue()
    {
        // Arrange
        var catalog = Catalog.Create("Old", "Old description");
        _repository.GetAsync(catalog.Id, CancellationToken.None).Returns(catalog);
        var handler = new UpdateCatalogHandler(_repository, _publisher);
        var request = new UpdateCatalogRequest { Name = "New", Description = "New description" };

        // Act
        var result = await handler.HandleAsync(catalog.Id, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(
            Arg.Is<Catalog>(c => c.Id == catalog.Id && c.Name == "New" && c.Description == "New description"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_ExistingCatalog_When_Handle_Then_PublishesCatalogUpdated()
    {
        // Arrange
        var catalog = Catalog.Create("Old", "Old description");
        _repository.GetAsync(catalog.Id, CancellationToken.None).Returns(catalog);
        var handler = new UpdateCatalogHandler(_repository, _publisher);
        var request = new UpdateCatalogRequest { Name = "New", Description = "New description" };

        // Act
        await handler.HandleAsync(catalog.Id, request, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is CatalogUpdated
                && ((CatalogUpdated)e).Id == catalog.Id
                && ((CatalogUpdated)e).Name == "New"
                && ((CatalogUpdated)e).Description == "New description"
                && e.Topic == Topics.CATALOGS),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_MissingCatalog_When_Handle_Then_ReturnsFalseWithoutUpdatingOrPublishing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetAsync(id, CancellationToken.None).Returns((Catalog?)null);
        var handler = new UpdateCatalogHandler(_repository, _publisher);

        // Act
        var result = await handler.HandleAsync(
            id, new UpdateCatalogRequest { Name = "New" }, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Catalog>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
