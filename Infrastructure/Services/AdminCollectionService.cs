using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminCollectionService : IAdminCollectionService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;
    private readonly ISlugService _slugService;

    public AdminCollectionService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService, ISlugService slugService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _slugService = slugService;
    }

    public async Task<CollectionListViewModel> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var collections = await _dbContext.Collections
            .AsNoTracking()
            .Include(x => x.ProductCollections)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = collections.Select(collection => new CollectionListItemViewModel
        {
            Id = collection.Id,
            Name = collection.Name,
            Slug = collection.Slug,
            Description = collection.Description,
            IsActive = collection.IsActive,
            ShowOnHome = collection.ShowOnHome,
            SortOrder = collection.SortOrder,
            ProductCount = collection.ProductCollections.Count
        }).ToList();

        return new CollectionListViewModel
        {
            TotalCount = items.Count,
            ActiveCount = items.Count(x => x.IsActive),
            HomeVisibleCount = items.Count(x => x.ShowOnHome),
            Collections = items
        };
    }

    public async Task<CollectionFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return new CollectionFormViewModel
        {
            ProductOptions = await GetProductOptionsAsync(cancellationToken)
        };
    }

    public async Task<CollectionFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var collection = await _dbContext.Collections
            .AsNoTracking()
            .Include(x => x.ProductCollections)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (collection is null)
        {
            return null;
        }

        return new CollectionFormViewModel
        {
            Id = collection.Id,
            Name = collection.Name,
            Slug = collection.Slug,
            Description = collection.Description,
            ShowOnHome = collection.ShowOnHome,
            SortOrder = collection.SortOrder,
            IsActive = collection.IsActive,
            ProductOptions = await GetProductOptionsAsync(cancellationToken),
            SelectedProductIds = collection.ProductCollections
                .Select(x => x.ProductId)
                .ToArray()
        };
    }

    public async Task<int> CreateAsync(CollectionFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Collection, cancellationToken: cancellationToken);

        var entity = new Collection
        {
            Name = model.Name.Trim(),
            Slug = normalizedSlug,
            Description = NormalizeDescription(model.Description),
            ShowOnHome = model.ShowOnHome,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive,
            UpdatedAt = DateTime.UtcNow
        };

        ApplyProducts(entity, model.SelectedProductIds);

        _dbContext.Collections.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateStorefrontAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(CollectionFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Collections
            .Include(x => x.ProductCollections)
            .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Collection, entity.Id, cancellationToken);

        entity.Name = model.Name.Trim();
        entity.Slug = normalizedSlug;
        entity.Description = NormalizeDescription(model.Description);
        entity.ShowOnHome = model.ShowOnHome;
        entity.SortOrder = model.SortOrder;
        entity.IsActive = model.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        ApplyProducts(entity, model.SelectedProductIds);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateStorefrontAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<CollectionProductOptionViewModel>> GetProductOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .OrderBy(x => x.Name)
            .Select(x => new CollectionProductOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                CategoryName = x.Category != null ? x.Category.Name : "-",
                BrandName = x.Brand != null ? x.Brand.Name : "-",
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeDescription(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static void ApplyProducts(Collection entity, IReadOnlyList<int>? selectedProductIds)
    {
        var normalizedProductIds = selectedProductIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        var currentProductIds = entity.ProductCollections.Select(x => x.ProductId).ToList();
        foreach (var productId in currentProductIds.Where(productId => !normalizedProductIds.Contains(productId)))
        {
            var relation = entity.ProductCollections.First(x => x.ProductId == productId);
            entity.ProductCollections.Remove(relation);
        }

        foreach (var productId in normalizedProductIds.Where(productId => currentProductIds.All(existing => existing != productId)))
        {
            entity.ProductCollections.Add(new ProductCollection
            {
                CollectionId = entity.Id,
                ProductId = productId
            });
        }
    }
}
