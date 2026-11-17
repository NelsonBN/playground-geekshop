using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>
/// One disposable SQL Server container shared by every integration test class in the collection.
/// The schema comes from <c>tests/init-db.sql</c>, the same script docker-compose runs.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string DATABASE = "GeekShop";

    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .Build();

    public string ConnectionString => new SqlConnectionStringBuilder(_container.GetConnectionString())
    {
        InitialCatalog = DATABASE
    }.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "init-db.sql"));
        var result = await _container.ExecScriptAsync(script);
        if(result.ExitCode != 0)
        {
            throw new InvalidOperationException($"tests/init-db.sql failed: {result.Stdout}{result.Stderr}");
        }
    }

    public async ValueTask DisposeAsync()
        => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(DatabaseCollection))]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
