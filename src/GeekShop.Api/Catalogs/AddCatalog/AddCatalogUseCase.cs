namespace GeekShop.Api.Catalogs.AddCatalog;

public sealed class AddCatalogUseCase(ICatalogsRepository repository)
{
    public async Task<Guid> HandleAsync(AddCatalogRequest request, CancellationToken cancellationToken)
    {
        var catalog = Catalog.Create(request.Name, request.Description);

        await repository.AddAsync(catalog, cancellationToken);

        return catalog.Id;
    }
}
