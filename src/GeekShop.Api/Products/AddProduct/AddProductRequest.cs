namespace GeekShop.Api.Products.AddProduct;

public sealed record AddProductRequest
{
    public required string Name { get; init; }
    public uint Quantity { get; init; }
    public double Price { get; init; }
}
