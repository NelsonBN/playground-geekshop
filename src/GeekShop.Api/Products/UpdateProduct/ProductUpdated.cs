using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.UpdateProduct;

public sealed record ProductUpdated(Guid Id, string Name, uint Quantity, double Price) : IntegrationEvent(Topics.PRODUCTS);
