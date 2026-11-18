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

        await repository.DeleteAsync(id, cancellationToken);

        return DeleteProductResult.Deleted;
    }
}
