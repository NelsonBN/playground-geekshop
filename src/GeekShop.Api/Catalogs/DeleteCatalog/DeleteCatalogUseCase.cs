namespace GeekShop.Api.Catalogs.DeleteCatalog;

public sealed class DeleteCatalogUseCase(ICatalogsRepository repository)
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
