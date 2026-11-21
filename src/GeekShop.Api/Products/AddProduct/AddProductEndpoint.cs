namespace GeekShop.Api.Products.AddProduct;

internal static class AddProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPost("/", async (
            AddProductHandler handler,
            AddProductRequest request,
            CancellationToken cancellationToken) =>
        {
            var id = await handler.HandleAsync(request, cancellationToken);

            return Results.CreatedAtRoute("GetProduct", new { id }, id);
        });
}
