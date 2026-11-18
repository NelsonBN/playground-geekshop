namespace GeekShop.Api.Catalogs.GetCatalogs;

internal static class GetCatalogsEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/", async (
            GetCatalogsUseCase useCase,
            CancellationToken cancellationToken)
            => Results.Ok(await useCase.HandleAsync(cancellationToken)));
}
