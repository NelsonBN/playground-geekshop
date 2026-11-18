namespace GeekShop.Api.Products;

public interface IProductsRepository
{
    Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken);
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    /// <summary>Deletes the product. Returns false, keeping it, when a catalog still references it.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
