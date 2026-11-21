using GeekShop.Api.Messaging;

namespace GeekShop.Api.Products.DeleteProduct;

public sealed record ProductDeleted(Guid Id) : IntegrationEvent(Topics.PRODUCTS);
