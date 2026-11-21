namespace GeekShop.Api.Messaging;

public static class Topics
{
    public const string PRODUCTS = "products";
    public const string CATALOGS = "catalogs";

    public static readonly IReadOnlyCollection<string> All = [PRODUCTS, CATALOGS];
}
