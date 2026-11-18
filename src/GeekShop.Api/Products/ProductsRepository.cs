using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GeekShop.Api.Products;

internal sealed class ProductsRepository(ProductsDbContext context) : IProductsRepository
{
    // SQL Server error: the statement conflicted with a FOREIGN KEY constraint.
    private const int FOREIGN_KEY_VIOLATION = 547;

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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([id], cancellationToken);
        if(product is null)
        {
            return true;
        }

        context.Products.Remove(product);

        try
        {
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch(DbUpdateException exception) when(exception.InnerException is SqlException { Number: FOREIGN_KEY_VIOLATION })
        {
            // A catalog still references the product (possibly added after the caller checked): keep it.
            return false;
        }
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => context.Products.AnyAsync(p => p.Id == id, cancellationToken);
}
