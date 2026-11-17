namespace GeekShop.Api.Products.DeleteProduct;

internal static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapDelete("/{id:guid}", async (
            DeleteProductUseCase useCase,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var deleted = await useCase.HandleAsync(id, cancellationToken);

            if(!deleted)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
}
