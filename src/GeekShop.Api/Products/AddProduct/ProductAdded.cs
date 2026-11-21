using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.AddProduct;

public sealed record ProductAdded(Guid Id, string Name, uint Quantity, double Price) : IntegrationEvent(Topics.PRODUCTS);
