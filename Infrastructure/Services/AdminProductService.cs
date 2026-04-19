using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Application.Utilities;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminProductService : IAdminProductService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;
    private readonly ISlugService _slugService;

    public AdminProductService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService, ISlugService slugService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _slugService = slugService;
    }

    public async Task<ProductListViewModel> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.ProductFeatures)
            .Include(x => x.ProductTags)
            .Include(x => x.ProductVariants)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = products.Select(product => new ProductListItemViewModel
        {
            Id = product.Id,
            ImageUrl = product.ImageUrl,
            Name = product.Name,
            Slug = product.Slug,
            BrandName = product.Brand?.Name ?? "-",
            CategoryName = product.Category?.Name ?? "-",
            Price = product.Price,
            Stock = product.Stock,
            StockSummary = BuildStockSummary(product),
            IsActive = product.IsActive,
            FeatureCount = product.ProductFeatures.Count,
            TagCount = product.ProductTags.Count,
            VariantCount = product.ProductVariants.Count,
            VariantSummary = BuildVariantSummary(product.ProductVariants)
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
            BrandOptions = await GetBrandOptionsAsync(cancellationToken),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            FeatureOptions = await GetFeatureOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken)
        };
    }

    public async Task<ProductFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductCategories)
            .Include(x => x.ProductFeatures)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductTags)
            .Include(x => x.ProductVariants.OrderBy(variant => variant.SortOrder).ThenBy(variant => variant.OptionName))
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
            ImageUrl = ProductMediaSync.GetOrderedUrls(product).FirstOrDefault() ?? product.ImageUrl,
            GalleryImageUrls = BuildLegacyGalleryImageUrls(product),
            MediaItemsJson = BuildMediaItemsJson(product),
            Stock = product.Stock,
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            BrandOptions = await GetBrandOptionsAsync(cancellationToken),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            FeatureOptions = await GetFeatureOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken),
            SelectedCategoryIds = product.ProductCategories.Select(x => x.CategoryId).DefaultIfEmpty(product.CategoryId).Distinct().ToArray(),
            Variants = product.ProductVariants
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.OptionName)
                .Select(x => new ProductVariantInputViewModel
                {
                    Id = x.Id,
                    GroupName = x.GroupName,
                    OptionName = x.OptionName,
                    Sku = x.Sku,
                    Price = x.Price,
                    OldPrice = x.OldPrice,
                    Stock = x.Stock,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToArray(),
            SelectedFeatureValues = product.ProductFeatures.ToDictionary(x => x.FeatureId, x => x.Value ?? string.Empty),
            SelectedFeatureIds = product.ProductFeatures.Select(x => x.FeatureId).ToArray(),
            SelectedTagIds = product.ProductTags.Select(x => x.TagId).ToArray()
        };
    }

    public async Task<int> CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Product, cancellationToken: cancellationToken);

        var entity = new Product
        {
            Name = model.Name.Trim(),
            Slug = normalizedSlug,
            Description = HtmlContentSanitizer.Sanitize(model.Description),
            Price = model.Price,
            OldPrice = NormalizeOldPrice(model.OldPrice),
            Rating = model.Rating,
            Stock = model.Stock,
            BrandId = model.BrandId,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive
        };

        _dbContext.Products.Add(entity);
        ApplyProductCategories(entity, model.CategoryId, model.SelectedCategoryIds);
        ApplyProductFeatures(entity, model.SelectedFeatureIds, model.SelectedFeatureValues);
        ApplyProductTags(entity, model.SelectedTagIds);
        ApplyProductMedia(entity, model.MediaItemsJson, model.ImageUrl, model.GalleryImageUrls);
        ApplyProductVariants(entity, model.Variants);
        ApplyVariantSummary(entity, model);
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
            .Include(x => x.ProductCategories)
            .Include(x => x.ProductFeatures)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductTags)
            .Include(x => x.ProductVariants)
            .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Product, entity.Id, cancellationToken);

        entity.Name = model.Name.Trim();
        entity.Slug = normalizedSlug;
        entity.Description = HtmlContentSanitizer.Sanitize(model.Description);
        entity.Price = model.Price;
        entity.OldPrice = NormalizeOldPrice(model.OldPrice);
        entity.Rating = model.Rating;
        entity.Stock = model.Stock;
        entity.BrandId = model.BrandId;
        entity.CategoryId = model.CategoryId;
        entity.IsActive = model.IsActive;
        ApplyProductCategories(entity, model.CategoryId, model.SelectedCategoryIds);
        ApplyProductFeatures(entity, model.SelectedFeatureIds, model.SelectedFeatureValues);
        ApplyProductTags(entity, model.SelectedTagIds);
        ApplyProductMedia(entity, model.MediaItemsJson, model.ImageUrl, model.GalleryImageUrls);
        ApplyProductVariants(entity, model.Variants);
        ApplyVariantSummary(entity, model);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateProductAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<ProductCategoryOptionViewModel>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ParentId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentId)
            .ThenBy(x => x.Name)
            .Select(x => new ProductCategoryOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProductBrandOptionViewModel>> GetBrandOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ProductBrandOptionViewModel
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

    private async Task<IReadOnlyList<ProductFeatureOptionViewModel>> GetFeatureOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Features
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GroupName)
            .ThenBy(x => x.Name)
            .Select(x => new ProductFeatureOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                GroupName = x.GroupName,
                OptionsPreview = BuildFeatureOptionsPreview(x.OptionsContent),
                Options = ParseFeatureOptions(x.OptionsContent)
            })
            .ToListAsync(cancellationToken);
    }

    private static decimal? NormalizeOldPrice(decimal? oldPrice)
    {
        return oldPrice is > 0 ? oldPrice : null;
    }

    private static string? NormalizeGalleryImageUrls(string? rawValue)
    {
        var urls = ProductMediaSync.NormalizeLegacy(null, rawValue)
            .Select(item => item.Url)
            .ToArray();

        return urls.Length == 0 ? null : string.Join(Environment.NewLine, urls);
    }

    private static string? BuildLegacyGalleryImageUrls(Product product)
    {
        var orderedUrls = ProductMediaSync.GetOrderedUrls(product);
        return orderedUrls.Count <= 1
            ? null
            : string.Join(Environment.NewLine, orderedUrls.Skip(1));
    }

    private static string BuildMediaItemsJson(Product product)
    {
        var items = product.ProductMedias
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new { url = x.Url, assetId = x.MediaAssetId })
            .ToArray();

        return JsonSerializer.Serialize(items);
    }

    private void ApplyProductMedia(Product entity, string? mediaItemsJson, string? imageUrl, string? galleryImageUrls)
    {
        var normalizedMedia = ProductMediaSync.Normalize(mediaItemsJson, imageUrl, galleryImageUrls);
        var assetIds = normalizedMedia
            .Where(item => item.AssetId.HasValue)
            .Select(item => item.AssetId!.Value)
            .Distinct()
            .ToArray();
        var assetLookup = _dbContext.MediaAssets
            .AsNoTracking()
            .Where(asset => assetIds.Contains(asset.Id))
            .ToDictionary(asset => asset.Id);

        var incomingKeys = normalizedMedia
            .Select(item => item.AssetId.HasValue ? $"asset:{item.AssetId.Value}" : $"url:{item.Url}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var removableMedia = entity.ProductMedias
            .Where(item =>
            {
                var key = item.MediaAssetId.HasValue ? $"asset:{item.MediaAssetId.Value}" : $"url:{item.Url}";
                return !incomingKeys.Contains(key);
            })
            .ToList();

        foreach (var media in removableMedia)
        {
            entity.ProductMedias.Remove(media);
        }

        foreach (var item in normalizedMedia)
        {
            var existing = item.AssetId.HasValue
                ? entity.ProductMedias.FirstOrDefault(media => media.MediaAssetId == item.AssetId.Value)
                : entity.ProductMedias.FirstOrDefault(media => !media.MediaAssetId.HasValue && string.Equals(media.Url, item.Url, StringComparison.OrdinalIgnoreCase));
            assetLookup.TryGetValue(item.AssetId ?? 0, out var linkedAsset);

            if (existing is null)
            {
                entity.ProductMedias.Add(new ProductMedia
                {
                    MediaAssetId = item.AssetId,
                    Url = linkedAsset?.Url ?? item.Url,
                    SortOrder = item.SortOrder,
                    IsPrimary = item.IsPrimary,
                    AltText = linkedAsset?.AltText ?? (string.IsNullOrWhiteSpace(entity.Name) ? "Urun gorseli" : entity.Name),
                    UpdatedAt = DateTime.UtcNow
                });
                continue;
            }

            existing.MediaAssetId = item.AssetId;
            existing.Url = linkedAsset?.Url ?? item.Url;
            existing.SortOrder = item.SortOrder;
            existing.IsPrimary = item.IsPrimary;
            existing.AltText = string.IsNullOrWhiteSpace(linkedAsset?.AltText)
                ? (string.IsNullOrWhiteSpace(existing.AltText) ? entity.Name : existing.AltText)
                : linkedAsset!.AltText;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        ProductMediaSync.SyncLegacyFields(entity);
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

    private void ApplyProductFeatures(Product entity, IReadOnlyList<int>? selectedFeatureIds, IReadOnlyDictionary<int, string>? selectedFeatureValues)
    {
        var normalizedFeatureIds = selectedFeatureIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        var currentFeatureIds = entity.ProductFeatures.Select(x => x.FeatureId).ToList();
        foreach (var featureId in currentFeatureIds.Where(featureId => !normalizedFeatureIds.Contains(featureId)))
        {
            var relation = entity.ProductFeatures.First(x => x.FeatureId == featureId);
            entity.ProductFeatures.Remove(relation);
        }

        foreach (var featureId in normalizedFeatureIds.Where(featureId => currentFeatureIds.All(existing => existing != featureId)))
        {
            entity.ProductFeatures.Add(new ProductFeature
            {
                ProductId = entity.Id,
                FeatureId = featureId,
                Value = NormalizeFeatureValue(selectedFeatureValues, featureId)
            });
        }

        foreach (var relation in entity.ProductFeatures.Where(x => normalizedFeatureIds.Contains(x.FeatureId)))
        {
            relation.Value = NormalizeFeatureValue(selectedFeatureValues, relation.FeatureId);
        }
    }

    private static string? NormalizeFeatureValue(IReadOnlyDictionary<int, string>? selectedFeatureValues, int featureId)
    {
        if (selectedFeatureValues is null || !selectedFeatureValues.TryGetValue(featureId, out var rawValue))
        {
            return null;
        }

        var normalized = rawValue?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static IReadOnlyList<string> ParseFeatureOptions(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<string>();
        }

        return rawValue
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? BuildFeatureOptionsPreview(string? rawValue)
    {
        var options = ParseFeatureOptions(rawValue).Take(3).ToArray();
        return options.Length == 0 ? null : string.Join(", ", options);
    }

    private static void ApplyProductCategories(Product entity, int primaryCategoryId, IReadOnlyList<int>? selectedCategoryIds)
    {
        var normalizedCategoryIds = selectedCategoryIds?
            .Where(x => x > 0)
            .Append(primaryCategoryId)
            .Distinct()
            .ToHashSet() ?? new HashSet<int> { primaryCategoryId };

        var currentCategoryIds = entity.ProductCategories.Select(x => x.CategoryId).ToList();
        foreach (var categoryId in currentCategoryIds.Where(categoryId => !normalizedCategoryIds.Contains(categoryId)))
        {
            var relation = entity.ProductCategories.First(x => x.CategoryId == categoryId);
            entity.ProductCategories.Remove(relation);
        }

        foreach (var categoryId in normalizedCategoryIds.Where(categoryId => currentCategoryIds.All(existing => existing != categoryId)))
        {
            entity.ProductCategories.Add(new ProductCategory
            {
                ProductId = entity.Id,
                CategoryId = categoryId
            });
        }
    }

    private static string BuildStockSummary(Product product)
    {
        var activeVariants = product.ProductVariants
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.OptionName)
            .ToList();

        if (activeVariants.Count == 0)
        {
            return "Ana stok";
        }

        var minimumStock = activeVariants.Min(x => x.Stock);
        return $"{product.Stock} toplam / min {minimumStock}";
    }

    private static string BuildVariantSummary(ICollection<ProductVariant> variants)
    {
        var activeCount = variants.Count(x => x.IsActive);
        return activeCount == 0 ? "-" : $"{activeCount} aktif";
    }

    private static void ApplyProductVariants(Product entity, IReadOnlyList<ProductVariantInputViewModel>? variants)
    {
        var normalizedVariants = variants?
            .Where(x => !string.IsNullOrWhiteSpace(x.GroupName) && !string.IsNullOrWhiteSpace(x.OptionName))
            .Select((x, index) => new ProductVariantInputViewModel
            {
                Id = x.Id,
                GroupName = x.GroupName.Trim(),
                OptionName = x.OptionName.Trim(),
                Sku = string.IsNullOrWhiteSpace(x.Sku) ? null : x.Sku.Trim(),
                Price = x.Price,
                OldPrice = NormalizeOldPrice(x.OldPrice),
                Stock = x.Stock,
                SortOrder = x.SortOrder == 0 ? index : x.SortOrder,
                IsActive = x.IsActive
            })
            .ToList() ?? new List<ProductVariantInputViewModel>();

        var incomingIds = normalizedVariants
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        var removableVariants = entity.ProductVariants
            .Where(x => !incomingIds.Contains(x.Id))
            .ToList();

        foreach (var removable in removableVariants)
        {
            entity.ProductVariants.Remove(removable);
        }

        foreach (var item in normalizedVariants)
        {
            var existing = item.Id.HasValue
                ? entity.ProductVariants.FirstOrDefault(x => x.Id == item.Id.Value)
                : null;

            if (existing is null)
            {
                entity.ProductVariants.Add(new ProductVariant
                {
                    GroupName = item.GroupName,
                    OptionName = item.OptionName,
                    Sku = item.Sku,
                    Price = item.Price,
                    OldPrice = item.OldPrice,
                    Stock = item.Stock,
                    SortOrder = item.SortOrder,
                    IsActive = item.IsActive,
                    UpdatedAt = DateTime.UtcNow
                });

                continue;
            }

            existing.GroupName = item.GroupName;
            existing.OptionName = item.OptionName;
            existing.Sku = item.Sku;
            existing.Price = item.Price;
            existing.OldPrice = item.OldPrice;
            existing.Stock = item.Stock;
            existing.SortOrder = item.SortOrder;
            existing.IsActive = item.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void ApplyVariantSummary(Product entity, ProductFormViewModel model)
    {
        var activeVariants = entity.ProductVariants
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.OptionName)
            .ToList();

        if (activeVariants.Count == 0)
        {
            entity.Price = model.Price;
            entity.OldPrice = NormalizeOldPrice(model.OldPrice);
            entity.Stock = model.Stock;
            return;
        }

        var primaryVariant = activeVariants[0];
        entity.Price = primaryVariant.Price;
        entity.OldPrice = primaryVariant.OldPrice;
        entity.Stock = activeVariants.Sum(x => x.Stock);
    }
}

