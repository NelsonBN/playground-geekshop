namespace GeekShop.Api.Products.DeleteProduct;

internal static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapDelete("/{id:guid}", async (
            DeleteProductUseCase useCase,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(id, cancellationToken);

            return result switch
            {
                DeleteProductResult.NotFound => Results.NotFound(),
                DeleteProductResult.InCatalog => Results.Conflict(),
                _ => Results.NoContent()
            };
        });
}
