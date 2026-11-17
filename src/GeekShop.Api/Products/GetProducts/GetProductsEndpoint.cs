namespace GeekShop.Api.Products.GetProducts;

internal static class GetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/", async (GetProductsUseCase useCase, CancellationToken cancellationToken)
            => Results.Ok(await useCase.HandleAsync(cancellationToken)));
}
