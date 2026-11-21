namespace GeekShop.Api.Catalogs.GetCatalog;

public sealed class GetCatalogHandler(ICatalogsRepository repository)
{
    public async Task<GetCatalogResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var catalog = await repository.GetAsync(id, cancellationToken);

        if(catalog is null)
        {
            return null;
        }

        var products = await repository.ListProductsAsync(id, cancellationToken);

        return new(
            catalog.Id,
            catalog.Name,
            catalog.Description,
            [.. products.Select(p => new CatalogProductResponse(p.Id, p.Name))]);
    }
}
