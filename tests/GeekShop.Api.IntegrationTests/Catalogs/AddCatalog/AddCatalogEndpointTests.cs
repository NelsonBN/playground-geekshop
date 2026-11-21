using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.AddCatalog;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace GeekShop.Api.IntegrationTests.Catalogs.AddCatalog;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class AddCatalogEndpointTests(DatabaseFixture database, RabbitMqFixture rabbitMq) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(database.ConnectionString, rabbitMq.ConnectionString);

    public async Task InitializeAsync()
        => await rabbitMq.SubscribeAsync(_factory);

    public async Task DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_ValidRequest_When_AddCatalog_Then_ReturnsCreatedAndPersistsCatalog()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = CatalogsApi.NewAddRequest();

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);

        // Assert
        response.Should().Be201Created();
        var id = await response.Content.ReadFromJsonAsync<Guid>(CancellationToken.None);
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
        stored.Should().BeEquivalentTo(new GetCatalogResponse(id, request.Name, request.Description, []));
    }

    [Fact]
    public async Task Given_LimitLengths_When_AddCatalog_Then_ReturnsCreatedAndPersistsThem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddCatalogRequest
        {
            Name = new string('n', Catalog.NAME_MAX_LENGTH),
            Description = new string('d', Catalog.DESCRIPTION_MAX_LENGTH)
        };

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);

        // Assert
        response.Should().Be201Created();
        var id = await response.Content.ReadFromJsonAsync<Guid>(CancellationToken.None);
        var stored = await client.GetFromJsonAsync<GetCatalogResponse>(
            $"/catalogs/{id}", CancellationToken.None);
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
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Fact]
    public async Task Given_BlankName_When_AddCatalog_Then_ReturnsBadRequestWithTheReason()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddCatalogRequest { Name = " " };

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);

        // Assert
        response.Should().Be400BadRequest();
        (await response.Content.ReadAsStringAsync(CancellationToken.None))
            .Should().Contain("name is required");
    }

    [Fact]
    public async Task Given_InvalidCatalog_When_PostCatalog_Then_ReturnsBadRequestProblemWithTheReason()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new AddCatalogRequest { Name = " " };

        // Act
        var response = await client.PostAsJsonAsync("/catalogs", request, CancellationToken.None);

        // Assert
        response.Should().Be400BadRequest();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken.None);
        problem.Should().BeEquivalentTo(new { Title = "Invalid catalog", Detail = "The catalog name is required." });
    }

    [Fact]
    public async Task Given_ValidRequest_When_PostCatalog_Then_PublishesCatalogAdded()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = CatalogsApi.NewAddRequest();

        // Act
        var id = await CatalogsApi.CreateAsync(client, request);

        // Assert
        var received = await rabbitMq.ExpectAsync<CatalogAdded>(Topics.CATALOGS, id);
        received.Should().BeEquivalentTo(
            new CatalogAdded(id, request.Name, request.Description), BrokerEvents.IgnoreEnvelope);
    }
}
