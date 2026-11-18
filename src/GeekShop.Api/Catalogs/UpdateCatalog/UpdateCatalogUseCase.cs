namespace GeekShop.Api.Catalogs.UpdateCatalog;

public sealed class UpdateCatalogUseCase(ICatalogsRepository repository)
{
    public async Task<bool> HandleAsync(Guid id, UpdateCatalogRequest request, CancellationToken cancellationToken)
    {
        var catalog = await repository.GetAsync(id, cancellationToken);

        if(catalog is null)
        {
            return false;
        }

        catalog.Update(request.Name, request.Description);

        await repository.UpdateAsync(catalog, cancellationToken);

        return true;
    }
}
