namespace GeekShop.Api.Products.UpdateProduct;

public sealed record UpdateProductRequest
{
    public required string Name { get; init; }
    public uint Quantity { get; init; }
    public double Price { get; init; }
}
