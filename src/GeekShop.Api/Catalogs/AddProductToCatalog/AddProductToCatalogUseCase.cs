using GeekShop.Api.Products;

namespace GeekShop.Api.Catalogs.AddProductToCatalog;

public sealed class AddProductToCatalogUseCase(ICatalogsRepository catalogs, IProductsRepository products)
{
    public async Task<bool> HandleAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken)
    {
        if(!await catalogs.ExistsAsync(catalogId, cancellationToken)
            || !await products.ExistsAsync(productId, cancellationToken))
        {
            return false;
        }

        await catalogs.AddProductAsync(catalogId, productId, cancellationToken);

        return true;
    }
}
