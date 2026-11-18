namespace GeekShop.Api.Catalogs.AddCatalog;

public sealed class AddCatalogUseCase(ICatalogsRepository repository)
{
    public async Task<Guid> HandleAsync(AddCatalogRequest request, CancellationToken cancellationToken)
    {
        var catalog = new Catalog
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        await repository.AddAsync(catalog, cancellationToken);

        return catalog.Id;
    }
}
