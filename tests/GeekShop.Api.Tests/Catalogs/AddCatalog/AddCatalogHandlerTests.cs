using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.AddCatalog;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.Tests.Catalogs.AddCatalog;

public sealed class AddCatalogHandlerTests
{
    private readonly ICatalogsRepository _repository = Substitute.For<ICatalogsRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Given_Request_When_Handle_Then_PersistsCatalogAndReturnsItsId()
    {
        // Arrange
        var handler = new AddCatalogHandler(_repository, _publisher);
        var request = new AddCatalogRequest { Name = "Summer", Description = "Summer deals" };

        // Act
        var id = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Catalog>(c => c.Id == id && c.Name == "Summer" && c.Description == "Summer deals"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_Request_When_Handle_Then_PublishesCatalogAdded()
    {
        // Arrange
        var handler = new AddCatalogHandler(_repository, _publisher);
        var request = new AddCatalogRequest { Name = "Summer", Description = "Summer deals" };

        // Act
        var id = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEvent>(e => e is CatalogAdded
                && ((CatalogAdded)e).Id == id
                && ((CatalogAdded)e).Name == "Summer"
                && ((CatalogAdded)e).Description == "Summer deals"
                && e.Topic == Topics.CATALOGS),
            CancellationToken.None);
    }
}
