using GeekShop.Api.Messaging;

namespace GeekShop.Api.Catalogs.AddCatalog;

public sealed record CatalogAdded(Guid Id, string Name, string? Description) : IntegrationEvent(Topics.CATALOGS);
