using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.DeleteCatalog;

public sealed class DeleteCatalogHandler(ICatalogsRepository repository, IEventPublisher publisher)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        if(!await repository.ExistsAsync(id, cancellationToken))
        {
            return false;
        }

        await repository.DeleteAsync(id, cancellationToken);

        await publisher.PublishAsync(new CatalogDeleted(id), cancellationToken);

        return true;
    }
}
