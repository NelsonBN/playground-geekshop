using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.RemoveProductFromCatalog;

public sealed record ProductRemovedFromCatalog(Guid CatalogId, Guid ProductId) : IntegrationEvent(Topics.CATALOGS);
