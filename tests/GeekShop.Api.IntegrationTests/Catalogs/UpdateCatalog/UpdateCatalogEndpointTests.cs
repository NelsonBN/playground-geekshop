using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Catalogs.UpdateCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Catalogs.UpdateCatalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class UpdateCatalogEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_CreatedCatalog_When_UpdateCatalog_Then_ReturnsNoContentAndPersistsChanges()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        var update = new UpdateCatalogRequest { Name = $"Winter-{Guid.NewGuid()}", Description = "Winter deals" };

        // Act
        var response = await client.PutAsJsonAsync($"/catalogs/{id}", update, CancellationToken.None);

        // Assert
        response.Should().Be204NoContent();
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        stored.Should().BeEquivalentTo(new GetCatalogResponse(id, update.Name, "Winter deals", []));
    }

    [Fact]
    public async Task Given_NameOverTheLimit_When_UpdateCatalog_Then_ReturnsBadRequestAndKeepsTheCatalog()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = CatalogsApi.NewAddRequest();
        var id = await CatalogsApi.CreateAsync(client, request);
        var update = new UpdateCatalogRequest { Name = new string('n', 201) };

        // Act
        var response = await client.PutAsJsonAsync($"/catalogs/{id}", update, CancellationToken.None);

        // Assert
        response.Should().Be400BadRequest();
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        stored!.Name.Should().Be(request.Name);
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
            CancellationToken.None);

        // Assert
        response.Should().Be404NotFound();
    }

    [Fact]
    public async Task Given_ExistingCatalog_When_PutCatalog_Then_PublishesCatalogUpdated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = await CatalogsApi.CreateAsync(client, CatalogsApi.NewAddRequest());
        var request = new UpdateCatalogRequest { Name = "Winter", Description = "Winter deals" };

        // Act
        await client.PutAsJsonAsync($"/catalogs/{id}", request, CancellationToken.None);

        // Assert
        var received = await rabbitMq.ExpectAsync<CatalogUpdated>(Topics.CATALOGS, id);
        received.Should().BeEquivalentTo(new CatalogUpdated(id, "Winter", "Winter deals"), BrokerEvents.IgnoreEnvelope);
    }

    [Fact]
    public async Task Given_UnknownCatalog_When_PutCatalog_Then_PublishesNothing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknown = Guid.NewGuid();
        var request = new UpdateCatalogRequest { Name = "Winter" };

        // Act
        await client.PutAsJsonAsync($"/catalogs/{unknown}", request, CancellationToken.None);

        // Assert
        await rabbitMq.ExpectNothingAsync(Topics.CATALOGS, nameof(CatalogUpdated), unknown);
    }
}
