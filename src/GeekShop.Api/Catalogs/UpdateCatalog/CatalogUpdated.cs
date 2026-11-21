using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.UpdateCatalog;

public sealed record CatalogUpdated(Guid Id, string Name, string? Description) : IntegrationEvent(Topics.CATALOGS);
