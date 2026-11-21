namespace GeekShop.Api.Catalogs.AddCatalog;

internal static class AddCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPost("/", async (
            AddCatalogHandler handler,
            AddCatalogRequest request,
            CancellationToken cancellationToken) =>
        {
            var id = await handler.HandleAsync(request, cancellationToken);

            return Results.CreatedAtRoute("GetCatalog", new { id }, id);
        });
}
