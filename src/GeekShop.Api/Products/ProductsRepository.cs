using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Products;

internal sealed class ProductsRepository(ProductsDbContext context) : IProductsRepository
{
    public async Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken)
        => await context.Products.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
        => context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([id], cancellationToken);
        if(product is null)
        {
            return;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => context.Products.AnyAsync(p => p.Id == id, cancellationToken);
}
