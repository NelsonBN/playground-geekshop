using GeekShop.Api.Products;
using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Catalogs;

public sealed class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();

    // Read-only view of the products table, only used to join catalog memberships.
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Catalog>(catalog =>
        {
            catalog.ToTable("Catalogs");
            catalog.HasKey(c => c.Id);
            catalog.Property(c => c.Id).ValueGeneratedNever();
            catalog.Property(c => c.Name).IsRequired().HasMaxLength(Catalog.NAME_MAX_LENGTH);
            catalog.Property(c => c.Description).HasMaxLength(Catalog.DESCRIPTION_MAX_LENGTH);
        });

        modelBuilder.Entity<CatalogProduct>(link =>
        {
            link.ToTable("CatalogProducts");
            link.HasKey(l => new { l.CatalogId, l.ProductId });
        });

        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("Products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Id).ValueGeneratedNever();
        });
    }
}
