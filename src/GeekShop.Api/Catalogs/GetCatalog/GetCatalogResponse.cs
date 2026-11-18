namespace GeekShop.Api.Catalogs.GetCatalog;

public sealed record GetCatalogResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyCollection<CatalogProductResponse> Products);

public sealed record CatalogProductResponse(Guid Id, string Name);
