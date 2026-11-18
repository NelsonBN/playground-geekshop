using GeekShop.Api.Products;
using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Catalogs;

internal sealed class CatalogsRepository(CatalogsDbContext context) : ICatalogsRepository
{
    public async Task<IReadOnlyCollection<Catalog>> ListAsync(CancellationToken cancellationToken)
        => await context.Catalogs.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Catalog?> GetAsync(Guid id, CancellationToken cancellationToken)
        => context.Catalogs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Catalog catalog, CancellationToken cancellationToken)
    {
        context.Catalogs.Add(catalog);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Catalog catalog, CancellationToken cancellationToken)
    {
        context.Catalogs.Update(catalog);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var catalog = await context.Catalogs.FindAsync([id], cancellationToken);
        if(catalog is null)
        {
            return;
        }

        context.Catalogs.Remove(catalog);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => context.Catalogs.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task AddProductAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken)
    {
        if(await context.CatalogProducts.AnyAsync(
            l => l.CatalogId == catalogId && l.ProductId == productId, cancellationToken))
        {
            return;
        }

        context.CatalogProducts.Add(new CatalogProduct { CatalogId = catalogId, ProductId = productId });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveProductAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken)
    {
        var link = await context.CatalogProducts.FindAsync([catalogId, productId], cancellationToken);
        if(link is null)
        {
            return;
        }

        context.CatalogProducts.Remove(link);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> ListProductsAsync(
        Guid catalogId,
        CancellationToken cancellationToken)
        => await context.CatalogProducts
            .AsNoTracking()
            .Where(l => l.CatalogId == catalogId)
            .Join(context.Products, l => l.ProductId, p => p.Id, (_, p) => p)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Catalog>> ListByProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
        => await context.CatalogProducts
            .AsNoTracking()
            .Where(l => l.ProductId == productId)
            .Join(context.Catalogs, l => l.CatalogId, c => c.Id, (_, c) => c)
            .ToListAsync(cancellationToken);
}
