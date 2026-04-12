using vitacure.Domain.Entities;

namespace vitacure.Application.Abstractions;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsByCategorySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int categoryId, int excludedProductId, int take = 4, CancellationToken cancellationToken = default);
}
