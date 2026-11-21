namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>Shares the SQL Server container and the RabbitMQ broker across every integration test class.</summary>
[CollectionDefinition(nameof(IntegrationCollection))]
public sealed class IntegrationCollection : ICollectionFixture<DatabaseFixture>, ICollectionFixture<RabbitMqFixture>;
