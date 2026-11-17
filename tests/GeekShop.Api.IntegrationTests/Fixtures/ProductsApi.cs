using GeekShop.Api.Products.AddProduct;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>Shared request builders and HTTP helpers for the products endpoint tests.</summary>
public static class ProductsApi
{
    public static AddProductRequest NewAddRequest(string? name = null)
        => new() { Name = name ?? $"Keyboard-{Guid.NewGuid()}", Quantity = 5, Price = 19.9 };

    public static async Task<Guid> CreateAsync(HttpClient client, AddProductRequest request)
    {
        var response = await client.PostAsJsonAsync("/products", request, TestContext.Current.CancellationToken);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }
}
