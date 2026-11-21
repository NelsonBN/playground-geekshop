using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.AddProductToCatalog;

public sealed record ProductAddedToCatalog(Guid CatalogId, Guid ProductId) : IntegrationEvent(Topics.CATALOGS);
