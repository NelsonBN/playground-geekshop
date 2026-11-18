namespace GeekShop.Api.Catalogs.DeleteCatalog;

internal static class DeleteCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapDelete("/{id:guid}", async (
            DeleteCatalogUseCase useCase,
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
