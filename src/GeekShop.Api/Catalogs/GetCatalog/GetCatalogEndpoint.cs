namespace GeekShop.Api.Catalogs.GetCatalog;

internal static class GetCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/{id:guid}", async (
            GetCatalogHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var catalog = await handler.HandleAsync(id, cancellationToken);

            if(catalog is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(catalog);
        }).WithName("GetCatalog");
}
