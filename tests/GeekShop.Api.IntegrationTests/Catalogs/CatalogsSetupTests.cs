using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.AddCatalog;
using GeekShop.Api.Catalogs.AddProductToCatalog;
using GeekShop.Api.Catalogs.DeleteCatalog;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Catalogs.GetCatalogs;
using GeekShop.Api.Catalogs.RemoveProductFromCatalog;
using GeekShop.Api.Catalogs.UpdateCatalog;
using GeekShop.Api.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GeekShop.Api.IntegrationTests.Catalogs;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class CatalogsSetupTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_Application_When_ResolvingServices_Then_RepositoryAndUseCasesAreRegistered()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;

        // Act
        object[] resolved =
        [
            services.GetRequiredService<ICatalogsRepository>(),
            services.GetRequiredService<AddCatalogUseCase>(),
            services.GetRequiredService<AddProductToCatalogUseCase>(),
            services.GetRequiredService<DeleteCatalogUseCase>(),
            services.GetRequiredService<GetCatalogUseCase>(),
            services.GetRequiredService<GetCatalogsUseCase>(),
            services.GetRequiredService<RemoveProductFromCatalogUseCase>(),
            services.GetRequiredService<UpdateCatalogUseCase>()
        ];

        // Assert
        resolved.Should().AllSatisfy(service => service.Should().NotBeNull());
    }

    [Fact]
    public void Given_Application_When_ListingEndpoints_Then_ExposesTheCatalogRoutes()
    {
        // Arrange
        var endpoints = _factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<HttpMethodMetadata>() is not null)
            .Select(e => (
                Method: e.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.Single(),
                Pattern: e.RoutePattern.RawText!.TrimEnd('/')))
            .ToList();

        // Act & Assert
        endpoints.Should().Contain(
        [
            ("GET", "/catalogs"),
            ("GET", "/catalogs/{id:guid}"),
            ("POST", "/catalogs"),
            ("PUT", "/catalogs/{id:guid}"),
            ("DELETE", "/catalogs/{id:guid}"),
            ("PUT", "/catalogs/{id:guid}/products/{productId:guid}"),
            ("DELETE", "/catalogs/{id:guid}/products/{productId:guid}")
        ]);
    }
}
