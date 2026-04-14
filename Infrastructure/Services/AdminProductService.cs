using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminProductService : IAdminProductService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;

    public AdminProductService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<ProductListViewModel> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductTags)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = products.Select(product => new ProductListItemViewModel
        {
            Id = product.Id,
            ImageUrl = product.ImageUrl,
            Name = product.Name,
            Slug = product.Slug,
            CategoryName = product.Category?.Name ?? "-",
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            TagCount = product.ProductTags.Count
        }).ToList();

        return new ProductListViewModel
        {
            TotalCount = items.Count,
            ActiveCount = items.Count(x => x.IsActive),
            OutOfStockCount = items.Count(x => x.Stock <= 0),
            Products = items
        };
    }

    public async Task<ProductFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return new ProductFormViewModel
        {
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken)
        };
    }

    public async Task<ProductFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductTags)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Rating = product.Rating,
            ImageUrl = product.ImageUrl,
            GalleryImageUrls = product.GalleryImageUrls,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken),
            SelectedTagIds = product.ProductTags.Select(x => x.TagId).ToArray()
        };
    }

    public async Task<int> CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = new Product
        {
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Description = model.Description.Trim(),
            Price = model.Price,
            OldPrice = NormalizeOldPrice(model.OldPrice),
            Rating = model.Rating,
            ImageUrl = model.ImageUrl.Trim(),
            GalleryImageUrls = NormalizeGalleryImageUrls(model.GalleryImageUrls),
            Stock = model.Stock,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive
        };

        _dbContext.Products.Add(entity);
        ApplyProductTags(entity, model.SelectedTagIds);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateProductAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Products
            .Include(x => x.ProductTags)
            .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Description = model.Description.Trim();
        entity.Price = model.Price;
        entity.OldPrice = NormalizeOldPrice(model.OldPrice);
        entity.Rating = model.Rating;
        entity.ImageUrl = model.ImageUrl.Trim();
        entity.GalleryImageUrls = NormalizeGalleryImageUrls(model.GalleryImageUrls);
        entity.Stock = model.Stock;
        entity.CategoryId = model.CategoryId;
        entity.IsActive = model.IsActive;
        ApplyProductTags(entity, model.SelectedTagIds);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateProductAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<ProductCategoryOptionViewModel>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ProductCategoryOptionViewModel
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProductTagOptionViewModel>> GetTagOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductTagOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug
            })
            .ToListAsync(cancellationToken);
    }

    private static decimal? NormalizeOldPrice(decimal? oldPrice)
    {
        return oldPrice is > 0 ? oldPrice : null;
    }

    private static string? NormalizeGalleryImageUrls(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var urls = rawValue
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return urls.Length == 0 ? null : string.Join(Environment.NewLine, urls);
    }

    private void ApplyProductTags(Product entity, IReadOnlyList<int>? selectedTagIds)
    {
        var normalizedTagIds = selectedTagIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        var currentTagIds = entity.ProductTags.Select(x => x.TagId).ToList();
        foreach (var tagId in currentTagIds.Where(tagId => !normalizedTagIds.Contains(tagId)))
        {
            var relation = entity.ProductTags.First(x => x.TagId == tagId);
            entity.ProductTags.Remove(relation);
        }

        foreach (var tagId in normalizedTagIds.Where(tagId => currentTagIds.All(existing => existing != tagId)))
        {
            entity.ProductTags.Add(new ProductTag
            {
                ProductId = entity.Id,
                TagId = tagId
            });
        }
    }
}
