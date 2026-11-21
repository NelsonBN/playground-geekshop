namespace GeekShop.Api.Catalogs.AddProductToCatalog;

internal static class AddProductToCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPut("/{id:guid}/products/{productId:guid}", async (
            AddProductToCatalogHandler handler,
            Guid id,
            Guid productId,
            CancellationToken cancellationToken) =>
        {
            var added = await handler.HandleAsync(id, productId, cancellationToken);

            if(!added)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
}
