namespace GeekShop.Api.Products.DeleteProduct;

public sealed class DeleteProductUseCase(IProductsRepository repository)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        if(!await repository.ExistsAsync(id, cancellationToken))
        {
            return false;
        }

        await repository.DeleteAsync(id, cancellationToken);

        return true;
    }
}
