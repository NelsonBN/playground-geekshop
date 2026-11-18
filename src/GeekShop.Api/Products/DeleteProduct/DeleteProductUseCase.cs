using GeekShop.Api.Catalogs;

namespace GeekShop.Api.Products.DeleteProduct;

public sealed class DeleteProductUseCase(IProductsRepository repository, ICatalogsRepository catalogs)
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
        return await repository.DeleteAsync(id, cancellationToken)
            ? DeleteProductResult.Deleted
            : DeleteProductResult.InCatalog;
    }
}
