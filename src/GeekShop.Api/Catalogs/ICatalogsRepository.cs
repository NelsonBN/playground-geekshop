using GeekShop.Api.Products;

namespace GeekShop.Api.Catalogs;

public interface ICatalogsRepository
{
    Task<IReadOnlyCollection<Catalog>> ListAsync(CancellationToken cancellationToken);
    Task<Catalog?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Catalog catalog, CancellationToken cancellationToken);
    Task UpdateAsync(Catalog catalog, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task AddProductAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken);
    Task RemoveProductAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Product>> ListProductsAsync(Guid catalogId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Catalog>> ListByProductAsync(Guid productId, CancellationToken cancellationToken);
}
