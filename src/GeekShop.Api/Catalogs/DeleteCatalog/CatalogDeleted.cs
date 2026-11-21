using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.DeleteCatalog;

public sealed record CatalogDeleted(Guid Id) : IntegrationEvent(Topics.CATALOGS);
