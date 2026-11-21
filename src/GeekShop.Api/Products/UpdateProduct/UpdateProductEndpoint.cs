namespace GeekShop.Api.Products.UpdateProduct;

internal static class UpdateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPut("/{id:guid}", async (
            UpdateProductHandler handler,
            Guid id,
            UpdateProductRequest request,
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
