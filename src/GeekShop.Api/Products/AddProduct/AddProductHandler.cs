using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.AddProduct;

public sealed class AddProductHandler(IProductsRepository repository, IEventPublisher publisher)
{
    public async Task<Guid> HandleAsync(AddProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Quantity = request.Quantity,
            Price = request.Price
        };

        await repository.AddAsync(product, cancellationToken);

        await publisher.PublishAsync(new ProductAdded(product.Id, product.Name, product.Quantity, product.Price), cancellationToken);

        return product.Id;
    }
}
