using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.UpdateCatalog;

public sealed class UpdateCatalogHandler(ICatalogsRepository repository, IEventPublisher publisher)
{
    public async Task<bool> HandleAsync(Guid id, UpdateCatalogRequest request, CancellationToken cancellationToken)
    {
        var catalog = await repository.GetAsync(id, cancellationToken);

        if(catalog is null)
        {
            return false;
        }

        catalog.Update(request.Name, request.Description);

        await repository.UpdateAsync(catalog, cancellationToken);

        await publisher.PublishAsync(new CatalogUpdated(catalog.Id, catalog.Name, catalog.Description), cancellationToken);

        return true;
    }
}
