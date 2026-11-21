namespace GeekShop.Api.Products.DeleteProduct;

internal static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapDelete("/{id:guid}", async (
            DeleteProductHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);

            return result switch
            {
                DeleteProductResult.NotFound => Results.NotFound(),
                DeleteProductResult.InCatalog => Results.Conflict(),
                _ => Results.NoContent()
            };
        });
}
