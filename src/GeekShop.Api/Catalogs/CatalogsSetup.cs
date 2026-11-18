using GeekShop.Api.Catalogs.AddCatalog;
using GeekShop.Api.Catalogs.AddProductToCatalog;
using GeekShop.Api.Catalogs.DeleteCatalog;
using GeekShop.Api.Catalogs.GetCatalog;
using GeekShop.Api.Catalogs.GetCatalogs;
using GeekShop.Api.Catalogs.RemoveProductFromCatalog;
using GeekShop.Api.Catalogs.UpdateCatalog;
using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Catalogs;

public static class CatalogsSetup
{
    public static IServiceCollection AddCatalogs(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDbContext<CatalogsDbContext>(options
                => options.UseSqlServer(configuration.GetConnectionString("GeekShop")))
            .AddScoped<ICatalogsRepository, CatalogsRepository>()
            .AddTransient<AddCatalogUseCase>()
            .AddTransient<AddProductToCatalogUseCase>()
            .AddTransient<DeleteCatalogUseCase>()
            .AddTransient<GetCatalogUseCase>()
            .AddTransient<GetCatalogsUseCase>()
            .AddTransient<RemoveProductFromCatalogUseCase>()
            .AddTransient<UpdateCatalogUseCase>();

    public static IEndpointRouteBuilder MapCatalogs(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/catalogs");

        GetCatalogsEndpoint.Map(group);
        GetCatalogEndpoint.Map(group);
        AddCatalogEndpoint.Map(group);
        UpdateCatalogEndpoint.Map(group);
        DeleteCatalogEndpoint.Map(group);
        AddProductToCatalogEndpoint.Map(group);
        RemoveProductFromCatalogEndpoint.Map(group);

        return endpoints;
    }
}
