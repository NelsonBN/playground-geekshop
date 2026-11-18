using System.Net;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Catalogs.UpdateCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.UpdateCatalog;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class UpdateCatalogEndpointTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedCatalog_When_UpdateCatalog_Then_ReturnsNoContentAndPersistsChanges()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        var update = new UpdateCatalogRequest { Name = $"Winter-{Guid.NewGuid()}", Description = "Winter deals" };

        // Act
        var response = await client.PutAsJsonAsync($"/catalogs/{id}", update, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", TestContext.Current.CancellationToken);
        stored.Should().BeEquivalentTo(new GetCatalogResponse(id, update.Name, "Winter deals", []));
    }

    [Fact]
    public async Task Given_UnknownId_When_UpdateCatalog_Then_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/catalogs/{Guid.NewGuid()}",
            new UpdateCatalogRequest { Name = "Winter" },
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
