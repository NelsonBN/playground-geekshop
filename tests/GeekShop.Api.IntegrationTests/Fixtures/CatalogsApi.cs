using GeekShop.Api.Catalogs.AddCatalog;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>Shared request builders and HTTP helpers for the catalogs endpoint tests.</summary>
public static class CatalogsApi
{
    public static AddCatalogRequest NewAddRequest(string? name = null)
        => new() { Name = name ?? $"Summer-{Guid.NewGuid()}", Description = "Summer deals" };

    public static async Task<Guid> CreateAsync(HttpClient client, AddCatalogRequest request)
    {
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);
        return await response.Content.ReadFromJsonAsync<Guid>(CancellationToken.None);
    }

    public static async Task AddProductAsync(HttpClient client, Guid catalogId, Guid productId)
        => await client.PutAsync(
            $"/catalogs/{catalogId}/products/{productId}", null, CancellationToken.None);
}
