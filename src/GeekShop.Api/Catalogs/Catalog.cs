namespace GeekShop.Api.Catalogs;

public sealed class Catalog
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
