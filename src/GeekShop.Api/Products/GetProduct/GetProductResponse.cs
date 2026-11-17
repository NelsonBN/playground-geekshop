namespace GeekShop.Api.Products.GetProduct;

public sealed record GetProductResponse(Guid Id, string Name, uint Quantity, double Price);
