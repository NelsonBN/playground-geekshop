namespace GeekShop.Api.Catalogs.UpdateCatalog;

public sealed record UpdateCatalogRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
