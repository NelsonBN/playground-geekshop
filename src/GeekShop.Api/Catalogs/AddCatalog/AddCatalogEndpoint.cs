namespace GeekShop.Api.Catalogs.AddCatalog;

internal static class AddCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapPost("/", async (
            AddCatalogUseCase useCase,
            AddCatalogRequest request,
            CancellationToken cancellationToken) =>
        {
            var id = await useCase.HandleAsync(request, cancellationToken);

            return Results.CreatedAtRoute("GetCatalog", new { id }, id);
        });
}
