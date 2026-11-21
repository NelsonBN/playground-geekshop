namespace GeekShop.Api.Catalogs.GetCatalogs;

internal static class GetCatalogsEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/", async (
            GetCatalogsHandler handler,
            CancellationToken cancellationToken)
            => Results.Ok(await handler.HandleAsync(cancellationToken)));
}
