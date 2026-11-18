using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.AddCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class AddCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ValidRequest_When_AddCatalog_Then_ReturnsCreatedAndPersistsCatalog()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = CatalogsApi.NewAddRequest();

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", TestContext.Current.CancellationToken);
        stored.Should().BeEquivalentTo(new GetCatalogResponse(id, request.Name, request.Description, []));
    }
}
