namespace GeekShop.Api.Products.GetProducts;

public sealed class GetProductsHandler(IProductsRepository repository)
{
    public async Task<IReadOnlyCollection<GetProductsResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(cancellationToken);

        return [.. products.Select(s => new GetProductsResponse(s.Id, s.Name))];
    }
}
