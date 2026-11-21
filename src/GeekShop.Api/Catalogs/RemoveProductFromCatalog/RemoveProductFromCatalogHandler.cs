using GeekShop.Api.Messaging;
using GeekShop.Api.Products;

namespace GeekShop.Api.Catalogs.RemoveProductFromCatalog;

public sealed class RemoveProductFromCatalogHandler(ICatalogsRepository catalogs, IProductsRepository products, IEventPublisher publisher)
{
    public async Task<bool> HandleAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken)
    {
        if(!await catalogs.ExistsAsync(catalogId, cancellationToken)
            || !await products.ExistsAsync(productId, cancellationToken))
        {
            return false;
        }

        await catalogs.RemoveProductAsync(catalogId, productId, cancellationToken);

        await publisher.PublishAsync(new ProductRemovedFromCatalog(catalogId, productId), cancellationToken);

        return true;
    }
}
