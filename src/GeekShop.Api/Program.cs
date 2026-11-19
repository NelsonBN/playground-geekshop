using GeekShop.Api.Catalogs;
using GeekShop.Api.Products;
using GeekShop.Api.Setup;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddProducts(builder.Configuration)
    .AddCatalogs(builder.Configuration)
    .AddHealth();

var app = builder.Build();

app
    .MapProducts()
    .MapCatalogs()
    .MapHealth();

await app.RunAsync();
