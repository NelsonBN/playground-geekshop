using GeekShop.Api.Catalogs;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.DeleteProduct;

public sealed class DeleteProductHandler(IProductsRepository repository, ICatalogsRepository catalogs, IEventPublisher publisher)
{
    public async Task<DeleteProductResult> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        if(!await repository.ExistsAsync(id, cancellationToken))
        {
            return DeleteProductResult.NotFound;
        }

        if((await catalogs.ListByProductAsync(id, cancellationToken)).Count > 0)
        {
            return DeleteProductResult.InCatalog;
        }

        // The repository also refuses when a catalog got the product after the check above.
        if(!await repository.DeleteAsync(id, cancellationToken))
        {
            return DeleteProductResult.InCatalog;
        }

        await publisher.PublishAsync(new ProductDeleted(id), cancellationToken);

        return DeleteProductResult.Deleted;
    }
}
