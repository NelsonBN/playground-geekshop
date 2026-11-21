using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.AddCatalog;

public sealed class AddCatalogHandler(ICatalogsRepository repository, IEventPublisher publisher)
{
    public async Task<Guid> HandleAsync(AddCatalogRequest request, CancellationToken cancellationToken)
    {
        var catalog = Catalog.Create(request.Name, request.Description);

        await repository.AddAsync(catalog, cancellationToken);

        await publisher.PublishAsync(new CatalogAdded(catalog.Id, catalog.Name, catalog.Description), cancellationToken);

        return catalog.Id;
    }
}
