using GeekShop.Api.Catalogs;

namespace GeekShop.Api.Products.GetProduct;

public sealed class GetProductUseCase(IProductsRepository repository, ICatalogsRepository catalogs)
{
    public async Task<GetProductResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(id, cancellationToken);

        if(product is null)
        {
            return null;
        }

        var productCatalogs = await catalogs.ListByProductAsync(id, cancellationToken);

        return new(
            product.Id,
            product.Name,
            product.Quantity,
            product.Price,
            [.. productCatalogs.Select(c => new ProductCatalogResponse(c.Id, c.Name))]);
    }
}
