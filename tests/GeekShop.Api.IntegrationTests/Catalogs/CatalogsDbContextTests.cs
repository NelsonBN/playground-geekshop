using GeekShop.Api.Catalogs;
using GeekShop.Api.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GeekShop.Api.IntegrationTests.Catalogs;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class CatalogsDbContextTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    [Fact]
    public async Task Given_Model_When_Inspected_Then_MapsCatalogsToTheSchemaTables()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogsDbContext>();

        // Act
        var catalog = context.Model.FindEntityType(typeof(Catalog))!;
        var link = context.Model.FindEntityType(typeof(CatalogProduct))!;

        // Assert
        catalog.GetTableName().Should().Be("Catalogs");
        catalog.FindProperty(nameof(Catalog.Name))!.GetMaxLength().Should().Be(200);
        catalog.FindProperty(nameof(Catalog.Name))!.IsNullable.Should().BeFalse();
        catalog.FindProperty(nameof(Catalog.Description))!.GetMaxLength().Should().Be(500);
        link.GetTableName().Should().Be("CatalogProducts");
        link.FindPrimaryKey()!.Properties.Select(p => p.Name)
            .Should().Equal(nameof(CatalogProduct.CatalogId), nameof(CatalogProduct.ProductId));
    }

    [Fact]
    public async Task Given_Schema_When_QueryingIndexes_Then_ProductIdLeadsAnIndexOfCatalogProducts()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogsDbContext>();

        // Act
        var indexes = await context.Database.SqlQuery<int>($"""
            SELECT COUNT(*) AS Value
            FROM sys.index_columns ic
            WHERE ic.object_id = OBJECT_ID(N'dbo.CatalogProducts')
              AND ic.key_ordinal = 1
              AND COL_NAME(ic.object_id, ic.column_id) = N'ProductId'
            """).SingleAsync(TestContext.Current.CancellationToken);

        // Assert
        indexes.Should().BePositive();
    }

    [Fact]
    public async Task Given_LongDescription_When_Saved_Then_DatabaseSchemaAcceptsTheModel()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogsDbContext>();
        var catalog = Catalog.Create(
            new string('n', Catalog.NAME_MAX_LENGTH),
            new string('d', Catalog.DESCRIPTION_MAX_LENGTH));

        // Act
        context.Catalogs.Add(catalog);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await context.Catalogs.AsNoTracking().FirstAsync(c => c.Id == catalog.Id, TestContext.Current.CancellationToken))
            .Description.Should().HaveLength(500);
    }
}
