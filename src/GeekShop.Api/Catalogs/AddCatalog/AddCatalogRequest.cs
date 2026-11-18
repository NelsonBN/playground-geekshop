namespace GeekShop.Api.Catalogs.AddCatalog;

public sealed record AddCatalogRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
