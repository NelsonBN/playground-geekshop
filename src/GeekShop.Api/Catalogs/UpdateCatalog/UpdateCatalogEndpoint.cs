namespace GeekShop.Api.Catalogs.UpdateCatalog;

internal static class UpdateCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPut("/{id:guid}", async (
            UpdateCatalogHandler handler,
            Guid id,
            UpdateCatalogRequest request,
            CancellationToken cancellationToken) =>
        {
            var updated = await handler.HandleAsync(id, request, cancellationToken);

            if(!updated)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
}
