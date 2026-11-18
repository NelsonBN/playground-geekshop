namespace GeekShop.Api.Products.GetProduct;

public sealed record GetProductResponse(
    Guid Id,
    string Name,
    uint Quantity,
    double Price,
    IReadOnlyCollection<ProductCatalogResponse> Catalogs);

public sealed record ProductCatalogResponse(Guid Id, string Name);
