namespace GeekShop.Api.Products.GetProduct;

public sealed class GetProductUseCase(IProductsRepository repository)
{
    public async Task<GetProductResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(id, cancellationToken);

        if(product is null)
        {
            return null;
        }

        return new(product.Id, product.Name, product.Quantity, product.Price);
    }
}
