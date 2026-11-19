namespace GeekShop.Api.Catalogs;

/// <summary>A catalog rule was broken. The API reports it as a bad request.</summary>
public sealed class InvalidCatalogException(string message) : Exception(message);
