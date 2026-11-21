namespace GeekShop.Api.Catalogs.RemoveProductFromCatalog;

internal static class RemoveProductFromCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapDelete("/{id:guid}/products/{productId:guid}", async (
            RemoveProductFromCatalogHandler handler,
            Guid id,
            Guid productId,
            CancellationToken cancellationToken) =>
        {
            var removed = await handler.HandleAsync(id, productId, cancellationToken);

            if(!removed)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
}
