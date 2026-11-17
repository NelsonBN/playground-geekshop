namespace GeekShop.Api.Products.AddProduct;

public sealed class AddProductUseCase(IProductsRepository repository)
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

        return product.Id;
    }
}
