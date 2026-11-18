using System.Net;
using GeekShop.Api.Catalogs.AddCatalog;
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

    [Theory]
    [InlineData(201, 0)]
    [InlineData(10, 501)]
    public async Task Given_NameOrDescriptionOverTheLimit_When_AddCatalog_Then_ReturnsBadRequest(
        int nameLength,
        int descriptionLength)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddCatalogRequest
        {
            Name = new string('n', nameLength),
            Description = new string('d', descriptionLength)
        };

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_BlankName_When_AddCatalog_Then_ReturnsBadRequestWithTheReason()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddCatalogRequest { Name = " " };

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().Contain("name is required");
    }
}
