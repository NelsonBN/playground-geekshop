namespace GeekShop.Api.Products.AddProduct;

internal static class AddProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPost("/", async (
            AddProductUseCase useCase,
            AddProductRequest request,
            CancellationToken cancellationToken) =>
        {
            var id = await useCase.HandleAsync(request, cancellationToken);

            return Results.CreatedAtRoute("GetProduct", new { id }, id);
        });
}
