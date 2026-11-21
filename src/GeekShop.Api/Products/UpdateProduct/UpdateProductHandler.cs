using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.UpdateProduct;

public sealed class UpdateProductHandler(IProductsRepository repository, IEventPublisher publisher)
{
    public async Task<bool> HandleAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(id, cancellationToken);

        if(product is null)
        {
            return false;
        }

        product.Name = request.Name;
        product.Quantity = request.Quantity;
        product.Price = request.Price;

        await repository.UpdateAsync(product, cancellationToken);

        await publisher.PublishAsync(new ProductUpdated(product.Id, product.Name, product.Quantity, product.Price), cancellationToken);

        return true;
    }
}
