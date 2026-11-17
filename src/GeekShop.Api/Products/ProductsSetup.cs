using GeekShop.Api.Products.AddProduct;
using GeekShop.Api.Products.DeleteProduct;
using GeekShop.Api.Products.GetProduct;
using GeekShop.Api.Products.GetProducts;
using GeekShop.Api.Products.UpdateProduct;
using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Products;

public static class ProductsSetup
{
    public static IServiceCollection AddProducts(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDbContext<ProductsDbContext>(options
                => options.UseSqlServer(configuration.GetConnectionString("GeekShop")))
            .AddScoped<IProductsRepository, ProductsRepository>()
            .AddTransient<AddProductUseCase>()
            .AddTransient<DeleteProductUseCase>()
            .AddTransient<GetProductUseCase>()
            .AddTransient<GetProductsUseCase>()
            .AddTransient<UpdateProductUseCase>();

    public static IEndpointRouteBuilder MapProducts(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/products");

        GetProductsEndpoint.Map(group);
        GetProductEndpoint.Map(group);
        AddProductEndpoint.Map(group);
        UpdateProductEndpoint.Map(group);
        DeleteProductEndpoint.Map(group);

        return endpoints;
    }
}
