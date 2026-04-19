using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;

namespace vitacure.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetProductsByCategorySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var categoryIds = await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.IsActive && x.Slug == slug)
            .Select(x => x.Id)
            .Take(1)
            .ToArrayAsync(cancellationToken);

        if (categoryIds.Length == 0)
        {
            return Array.Empty<Product>();
        }

        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductCategories)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductVariants)
            .Where(x => x.IsActive && (categoryIds.Contains(x.CategoryId) || x.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId))))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductVariants)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .Take(12)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag)
            .Include(x => x.ProductVariants)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(int categoryId, int excludedProductId, int take = 4, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductVariants)
            .Where(x => x.IsActive && x.CategoryId == categoryId && x.Id != excludedProductId)
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
