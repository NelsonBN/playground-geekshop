using GeekShop.Api.Catalogs.GetCatalogs;
using GeekShop.Api.IntegrationTests.Fixtures;

namespace GeekShop.Api.IntegrationTests.Catalogs.GetCatalogs;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class GetCatalogsEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedCatalog_When_GetCatalogs_Then_ReturnsOkIncludingIt()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = CatalogsApi.NewAddRequest();
        var id = await CatalogsApi.CreateAsync(client, request);

        // Act
        var response = await client.GetAsync("/catalogs", CancellationToken.None);

        // Assert
        response.Should().Be200Ok();
        var body = await response.Content.ReadFromJsonAsync<GetCatalogsResponse[]>(
            CancellationToken.None);
        body.Should().Contain(new GetCatalogsResponse(id, request.Name));
    }
}
