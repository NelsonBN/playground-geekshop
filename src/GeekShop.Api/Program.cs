using GeekShop.Api.Products;
using GeekShop.Api.Setup;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddProducts(builder.Configuration)
    .AddHealth();

var app = builder.Build();

app
    .MapProducts()
    .MapHealth();

app.Run();
