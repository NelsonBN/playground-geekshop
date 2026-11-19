namespace GeekShop.Api.Catalogs.GetCatalogs;

public sealed class GetCatalogsUseCase(ICatalogsRepository repository)
{
    public async Task<IReadOnlyCollection<GetCatalogsResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var catalogs = await repository.ListAsync(cancellationToken);

        return [.. catalogs.Select(c => new GetCatalogsResponse(c.Id, c.Name))];
    }
}
