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
            .AddTransient<AddCatalogHandler>()
            .AddTransient<AddProductToCatalogHandler>()
            .AddTransient<DeleteCatalogHandler>()
            .AddTransient<GetCatalogHandler>()
            .AddTransient<GetCatalogsHandler>()
            .AddTransient<RemoveProductFromCatalogHandler>()
            .AddTransient<UpdateCatalogHandler>();

    public static IEndpointRouteBuilder MapCatalogs(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/catalogs")
            .AddEndpointFilter(async (context, next) =>
            {
                try
                {
                    return await next(context);
                }
                catch(InvalidCatalogException exception)
                {
                    return Results.Problem(
                        title: "Invalid catalog",
                        detail: exception.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }
            });

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
