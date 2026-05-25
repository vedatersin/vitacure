using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Application.Utilities;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
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
                .ThenInclude(x => x.Tag)
            .Include(x => x.ProductVariants)
            .Include(x => x.ProductVariantGroups)
            .Include(x => x.ProductMedias)
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
            TagNames = product.ProductTags
                .Select(x => x.Tag?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToArray(),
            SalesChannels = Array.Empty<string>(),
            Price = product.Price,
            OldPrice = product.OldPrice,
            Stock = product.Stock,
            StockSummary = BuildStockSummary(product),
            IsActive = product.IsActive,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = ResolveUpdatedAt(product),
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
            BrandOptions = items
                .Select(x => x.BrandName)
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != "-")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            CategoryOptions = items
                .Select(x => x.CategoryName)
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != "-")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            TagOptions = items
                .SelectMany(x => x.TagNames)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            SalesChannelOptions = items
                .SelectMany(x => x.SalesChannels)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Products = items
        };
    }

    public async Task<ProductFormViewModel> GetCreateModelAsync(string? createMode = null, CancellationToken cancellationToken = default)
    {
        var normalizedCreateMode = NormalizeCreateMode(createMode);

        return new ProductFormViewModel
        {
            CreateMode = normalizedCreateMode,
            ProductKind = normalizedCreateMode is "bundle" or "bundle-variant" ? ProductKind.Bundle : ProductKind.Physical,
            BundleMode = normalizedCreateMode == "bundle-variant" ? "variant" : "simple",
            BundlePricingMode = "manual",
            BundleAdjustmentType = "none",
            Status = ProductPublishingStatus.PublishedOpen,
            BrandOptions = await GetBrandOptionsAsync(cancellationToken),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            GoogleProductCategoryOptions = await GetGoogleProductCategoryOptionsAsync(cancellationToken),
            FeatureOptions = await GetFeatureOptionsAsync(cancellationToken),
            CustomFieldOptions = await GetCustomFieldOptionsAsync(cancellationToken),
            PersonalizationOptions = await GetPersonalizationOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken),
            VariantPresets = await GetVariantPresetsAsync(cancellationToken),
            BundleProductOptions = await GetBundleProductOptionsAsync(null, cancellationToken),
            GoogleProductCategoryId = await GetDefaultGoogleProductCategoryIdAsync(cancellationToken),
            VariantFieldVisibility = new ProductVariantFieldVisibilityViewModel(),
            Variants = Array.Empty<ProductVariantInputViewModel>(),
            VariantGroups = Array.Empty<ProductVariantGroupInputViewModel>(),
            BundleItems = Array.Empty<ProductBundleItemInputViewModel>()
        };
    }

    public async Task<ProductFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductCategories)
            .Include(x => x.ProductCustomFields)
                .ThenInclude(x => x.CustomFieldDefinition)
            .Include(x => x.ProductFeatures)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductPersonalizations)
                .ThenInclude(x => x.PersonalizationDefinition)
            .Include(x => x.ProductTags)
            .Include(x => x.ProductBundleItems)
                .ThenInclude(item => item.ChildProduct)
            .Include(x => x.ProductBundleItems)
                .ThenInclude(item => item.ChildProductVariant)
            .Include(x => x.ProductVariantGroups.OrderBy(group => group.SortOrder).ThenBy(group => group.Id))
                .ThenInclude(group => group.Options.OrderBy(option => option.SortOrder).ThenBy(option => option.Id))
            .Include(x => x.ProductVariants.OrderBy(variant => variant.SortOrder).ThenBy(variant => variant.OptionName))
                .ThenInclude(variant => variant.Selections)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new ProductFormViewModel
        {
            Id = product.Id,
            CreateMode = ResolveCreateMode(product),
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            ProductKind = product.ProductKind,
            BundleMode = NormalizeBundleMode(product.BundleMode, product.ProductVariants.Any()),
            BundlePricingMode = NormalizeBundlePricingMode(product.BundlePricingMode),
            BundleAdjustmentType = NormalizeBundleAdjustmentType(product.BundleAdjustmentType),
            BundleAdjustmentAmount = product.BundleAdjustmentAmount,
            BundleTotalQuantity = product.BundleTotalQuantity,
            Price = product.Price,
            OldPrice = product.OldPrice,
            PurchasePrice = product.PurchasePrice,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            ImageUrl = ProductMediaSync.GetOrderedUrls(product).FirstOrDefault() ?? product.ImageUrl,
            GalleryImageUrls = BuildLegacyGalleryImageUrls(product),
            MediaItemsJson = BuildMediaItemsJson(product),
            Stock = product.Stock,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Desi = product.Desi,
            HsCode = product.HsCode,
            SupplierName = product.SupplierName,
            VariantFieldVisibility = ParseVariantFieldVisibility(product.VariantFieldVisibilityJson),
            ShowUnitPrice = product.ShowUnitPrice,
            UnitContentAmount = product.UnitContentAmount,
            UnitContentType = product.UnitContentType,
            UnitComparisonAmount = product.UnitComparisonAmount,
            UnitComparisonType = product.UnitComparisonType,
            ContinueSellingWhenOutOfStock = product.ContinueSellingWhenOutOfStock,
            BrandId = product.BrandId,
            GoogleProductCategoryId = product.GoogleProductCategoryId,
            CategoryId = product.CategoryId,
            Status = product.Status,
            IsActive = product.IsActive,
            BrandOptions = await GetBrandOptionsAsync(cancellationToken),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            GoogleProductCategoryOptions = await GetGoogleProductCategoryOptionsAsync(cancellationToken),
            FeatureOptions = await GetFeatureOptionsAsync(cancellationToken),
            CustomFieldOptions = await GetCustomFieldOptionsAsync(cancellationToken),
            PersonalizationOptions = await GetPersonalizationOptionsAsync(cancellationToken),
            TagOptions = await GetTagOptionsAsync(cancellationToken),
            VariantPresets = await GetVariantPresetsAsync(cancellationToken),
            BundleProductOptions = await GetBundleProductOptionsAsync(product.Id, cancellationToken),
            SelectedCategoryIds = product.ProductCategories.Select(x => x.CategoryId).Distinct().ToArray(),
            SelectedCustomFieldIds = product.ProductCustomFields.Select(x => x.CustomFieldDefinitionId).Distinct().ToArray(),
            Variants = BuildVariantInputs(product),
            VariantGroups = BuildVariantGroups(product),
            BundleItems = BuildBundleItems(product),
            SelectedFeatureValues = product.ProductFeatures.ToDictionary(x => x.FeatureId, x => x.Value ?? string.Empty),
            SelectedFeatureIds = product.ProductFeatures.Select(x => x.FeatureId).ToArray(),
            SelectedPersonalizationIds = product.ProductPersonalizations.Select(x => x.PersonalizationDefinitionId).Distinct().ToArray(),
            SelectedTagIds = product.ProductTags.Select(x => x.TagId).ToArray()
        };
    }

    public async Task<int> CreateAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = await ResolveSlugAsync(model, cancellationToken);

        var entity = new Product
        {
            ProductKind = ResolveProductKind(model),
            Name = NormalizeFreeText(model.Name),
            Slug = normalizedSlug,
            Description = HtmlContentSanitizer.Sanitize(NormalizeFreeText(model.Description)),
            MetaTitle = NormalizeOptionalText(model.MetaTitle),
            MetaDescription = NormalizeOptionalText(model.MetaDescription),
            Price = NormalizeMoney(model.Price),
            OldPrice = NormalizeOldPrice(model.OldPrice),
            PurchasePrice = NormalizeMoney(model.PurchasePrice),
            Rating = NormalizeRating(model.Rating),
            ReviewCount = NormalizeReviewCount(model.ReviewCount),
            Stock = NormalizeStock(model.Stock),
            Sku = NormalizeOptionalText(model.Sku),
            Barcode = NormalizeOptionalText(model.Barcode),
            Desi = NormalizeDecimal(model.Desi),
            HsCode = NormalizeOptionalText(model.HsCode),
            SupplierName = NormalizeOptionalText(model.SupplierName),
            VariantFieldVisibilityJson = SerializeVariantFieldVisibility(model.VariantFieldVisibility),
            BundleMode = NormalizeBundleMode(model.BundleMode, model.CreateMode == "bundle-variant"),
            BundlePricingMode = NormalizeBundlePricingMode(model.BundlePricingMode),
            BundleAdjustmentType = NormalizeBundleAdjustmentType(model.BundleAdjustmentType),
            BundleAdjustmentAmount = NormalizeBundleAdjustmentAmount(model.BundleAdjustmentAmount),
            BundleTotalQuantity = NormalizeBundleTotalQuantity(model.BundleTotalQuantity),
            ContinueSellingWhenOutOfStock = model.ContinueSellingWhenOutOfStock,
            ShowUnitPrice = model.ShowUnitPrice,
            UnitContentAmount = NormalizeUnitAmount(model.ShowUnitPrice, model.UnitContentAmount),
            UnitContentType = NormalizeUnitType(model.ShowUnitPrice, model.UnitContentType),
            UnitComparisonAmount = NormalizeUnitAmount(model.ShowUnitPrice, model.UnitComparisonAmount),
            UnitComparisonType = NormalizeUnitType(model.ShowUnitPrice, model.UnitComparisonType),
            BrandId = model.BrandId,
            GoogleProductCategoryId = NormalizeCategoryId(model.GoogleProductCategoryId),
            CategoryId = NormalizeCategoryId(model.CategoryId),
            Status = model.Status,
            IsActive = model.Status.IsPubliclyVisible()
        };

        _dbContext.Products.Add(entity);
        ApplyProductCategories(entity, entity.CategoryId, model.SelectedCategoryIds);
        ApplyProductCustomFields(entity, model.SelectedCustomFieldIds);
        ApplyProductFeatures(entity, model.SelectedFeatureIds, model.SelectedFeatureValues);
        ApplyProductPersonalizations(entity, model.SelectedPersonalizationIds);
        ApplyProductTags(entity, model.SelectedTagIds);
        ApplyProductMedia(entity, model.MediaItemsJson, model.ImageUrl, model.GalleryImageUrls);
        ApplyProductVariants(entity, model.Variants, model.VariantGroups);
        ApplyBundleItems(entity, model.BundleItems);
        ApplyBundleSummary(entity, model);
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
            .Include(x => x.ProductCustomFields)
            .Include(x => x.ProductFeatures)
            .Include(x => x.ProductMedias)
            .Include(x => x.ProductPersonalizations)
            .Include(x => x.ProductBundleItems)
            .Include(x => x.ProductTags)
            .Include(x => x.ProductVariantGroups)
                .ThenInclude(group => group.Options)
            .Include(x => x.ProductVariants)
                .ThenInclude(variant => variant.Selections)
            .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = await ResolveSlugAsync(model, cancellationToken, entity.Id);

        entity.ProductKind = ResolveProductKind(model);
        entity.Name = NormalizeFreeText(model.Name);
        entity.Slug = normalizedSlug;
        entity.Description = HtmlContentSanitizer.Sanitize(NormalizeFreeText(model.Description));
        entity.MetaTitle = NormalizeOptionalText(model.MetaTitle);
        entity.MetaDescription = NormalizeOptionalText(model.MetaDescription);
        entity.Price = NormalizeMoney(model.Price);
        entity.OldPrice = NormalizeOldPrice(model.OldPrice);
        entity.PurchasePrice = NormalizeMoney(model.PurchasePrice);
        entity.Rating = NormalizeRating(model.Rating);
        entity.ReviewCount = NormalizeReviewCount(model.ReviewCount);
        entity.Stock = NormalizeStock(model.Stock);
        entity.Sku = NormalizeOptionalText(model.Sku);
        entity.Barcode = NormalizeOptionalText(model.Barcode);
        entity.Desi = NormalizeDecimal(model.Desi);
        entity.HsCode = NormalizeOptionalText(model.HsCode);
        entity.SupplierName = NormalizeOptionalText(model.SupplierName);
        entity.VariantFieldVisibilityJson = SerializeVariantFieldVisibility(model.VariantFieldVisibility);
        entity.BundleMode = NormalizeBundleMode(model.BundleMode, model.CreateMode == "bundle-variant");
        entity.BundlePricingMode = NormalizeBundlePricingMode(model.BundlePricingMode);
        entity.BundleAdjustmentType = NormalizeBundleAdjustmentType(model.BundleAdjustmentType);
        entity.BundleAdjustmentAmount = NormalizeBundleAdjustmentAmount(model.BundleAdjustmentAmount);
        entity.BundleTotalQuantity = NormalizeBundleTotalQuantity(model.BundleTotalQuantity);
        entity.ContinueSellingWhenOutOfStock = model.ContinueSellingWhenOutOfStock;
        entity.ShowUnitPrice = model.ShowUnitPrice;
        entity.UnitContentAmount = NormalizeUnitAmount(model.ShowUnitPrice, model.UnitContentAmount);
        entity.UnitContentType = NormalizeUnitType(model.ShowUnitPrice, model.UnitContentType);
        entity.UnitComparisonAmount = NormalizeUnitAmount(model.ShowUnitPrice, model.UnitComparisonAmount);
        entity.UnitComparisonType = NormalizeUnitType(model.ShowUnitPrice, model.UnitComparisonType);
        entity.BrandId = model.BrandId;
        entity.GoogleProductCategoryId = NormalizeCategoryId(model.GoogleProductCategoryId);
        entity.CategoryId = NormalizeCategoryId(model.CategoryId);
        entity.Status = model.Status;
        entity.IsActive = model.Status.IsPubliclyVisible();
        ApplyProductCategories(entity, entity.CategoryId, model.SelectedCategoryIds);
        ApplyProductCustomFields(entity, model.SelectedCustomFieldIds);
        ApplyProductFeatures(entity, model.SelectedFeatureIds, model.SelectedFeatureValues);
        ApplyProductPersonalizations(entity, model.SelectedPersonalizationIds);
        ApplyProductTags(entity, model.SelectedTagIds);
        ApplyProductMedia(entity, model.MediaItemsJson, model.ImageUrl, model.GalleryImageUrls);
        ApplyProductVariants(entity, model.Variants, model.VariantGroups);
        ApplyBundleItems(entity, model.BundleItems);
        ApplyBundleSummary(entity, model);
        ApplyVariantSummary(entity, model);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateProductAsync(cancellationToken);
        return true;
    }

    private static string NormalizeCreateMode(string? createMode)
    {
        return createMode?.Trim().ToLowerInvariant() switch
        {
            "variant" => "variant",
            "bundle-variant" => "bundle-variant",
            "bundle" => "bundle",
            _ => "simple"
        };
    }

    private static string ResolveCreateMode(Product product)
    {
        if (product.ProductKind == ProductKind.Bundle)
        {
            return product.ProductVariants.Any() || string.Equals(product.BundleMode, "variant", StringComparison.OrdinalIgnoreCase)
                ? "bundle-variant"
                : "bundle";
        }

        return product.ProductVariants.Any() ? "variant" : "simple";
    }

    private static ProductKind ResolveProductKind(ProductFormViewModel model)
    {
        return model.CreateMode is "bundle" or "bundle-variant" || model.ProductKind == ProductKind.Bundle
            ? ProductKind.Bundle
            : model.ProductKind;
    }

    private static string NormalizeBundleMode(string? mode, bool hasVariants)
    {
        return string.Equals(mode?.Trim(), "variant", StringComparison.OrdinalIgnoreCase) || hasVariants
            ? "variant"
            : "simple";
    }

    private static string NormalizeBundlePricingMode(string? mode)
    {
        return string.Equals(mode?.Trim(), "sum", StringComparison.OrdinalIgnoreCase)
            ? "sum"
            : "manual";
    }

    private static string NormalizeBundleAdjustmentType(string? mode)
    {
        return mode?.Trim().ToLowerInvariant() switch
        {
            "increase" => "increase",
            "discount" => "discount",
            _ => "none"
        };
    }

    private static decimal? NormalizeBundleAdjustmentAmount(decimal? amount)
    {
        return amount is > 0 ? amount : null;
    }

    private static int? NormalizeBundleTotalQuantity(int? totalQuantity)
    {
        return totalQuantity is > 0 ? totalQuantity : null;
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

    private async Task<IReadOnlyList<ProductGoogleCategoryOptionViewModel>> GetGoogleProductCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.GoogleProductCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProductGoogleCategoryOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<int?> GetDefaultGoogleProductCategoryIdAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.GoogleProductCategories
            .AsNoTracking()
            .Where(x => x.IsActive && x.ParentId == null)
            .Where(x => x.Name == "Saglik ve Guzellik" || x.Slug == "saglik-ve-guzellik")
            .OrderBy(x => x.SortOrder)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
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

    private async Task<IReadOnlyList<ProductBundleProductOptionViewModel>> GetBundleProductOptionsAsync(int? currentProductId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductVariants)
            .Where(x => !currentProductId.HasValue || x.Id != currentProductId.Value)
            .OrderBy(x => x.Name)
            .Select(x => new ProductBundleProductOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                Stock = x.Stock,
                HasVariants = x.ProductVariants.Any(variant => variant.IsActive),
                Variants = x.ProductVariants
                    .Where(variant => variant.IsActive)
                    .OrderBy(variant => variant.SortOrder)
                    .ThenBy(variant => variant.OptionName)
                    .Select(variant => new ProductBundleProductVariantOptionViewModel
                    {
                        Id = variant.Id,
                        Label = string.IsNullOrWhiteSpace(variant.DisplayName) ? variant.OptionName : variant.DisplayName,
                        Price = variant.Price,
                        Stock = variant.Stock
                    })
                    .ToArray()
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProductCustomFieldOptionViewModel>> GetCustomFieldOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CustomFieldDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ProductCustomFieldOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                FieldType = x.FieldType,
                IsFilterable = x.IsFilterable
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProductPersonalizationOptionViewModel>> GetPersonalizationOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.PersonalizationDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ProductPersonalizationOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                InputType = x.InputType
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

    private async Task<IReadOnlyList<ProductVariantPresetViewModel>> GetVariantPresetsAsync(CancellationToken cancellationToken)
    {
        var groupedPresets = await _dbContext.ProductVariantGroups
            .AsNoTracking()
            .Include(x => x.Options)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        if (groupedPresets.Count > 0)
        {
            return groupedPresets
                .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var source = group
                        .OrderByDescending(item => item.Options.Count)
                        .ThenBy(item => item.SortOrder)
                        .First();

                    return new ProductVariantPresetViewModel
                    {
                        Name = source.Name,
                        SelectionStyle = NormalizeSelectionStyle(source.SelectionStyle),
                        Options = source.Options
                            .OrderBy(option => option.SortOrder)
                            .ThenBy(option => option.Name)
                            .Select(option => new ProductVariantPresetOptionViewModel
                            {
                                Name = option.Name,
                                ColorHex = option.ColorHex,
                                SwatchImageUrl = option.SwatchImageUrl
                            })
                            .ToArray()
                    };
                })
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var legacyPresets = await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.GroupName) && !string.IsNullOrWhiteSpace(x.OptionName))
            .OrderBy(x => x.GroupName)
            .ThenBy(x => x.OptionName)
            .ToListAsync(cancellationToken);

        return legacyPresets
            .GroupBy(x => x.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new ProductVariantPresetViewModel
            {
                Name = group.Key,
                SelectionStyle = "list",
                Options = group
                    .Select(x => x.OptionName.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select(name => new ProductVariantPresetOptionViewModel
                    {
                        Name = name
                    })
                    .ToArray()
            })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<string> ResolveSlugAsync(ProductFormViewModel model, CancellationToken cancellationToken, int? productId = null)
    {
        var candidate = NormalizeOptionalText(model.Slug);
        if (string.IsNullOrWhiteSpace(candidate))
        {
            var nameSeed = NormalizeOptionalText(model.Name);
            candidate = string.IsNullOrWhiteSpace(nameSeed)
                ? $"draft-product-{Guid.NewGuid():N}"
                : Slugify(nameSeed);
        }

        await _slugService.EnsureAvailableAsync(candidate, SlugEntityType.Product, productId, cancellationToken);
        return candidate;
    }

    private static string NormalizeFreeText(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var buffer = new List<char>(normalized.Length);
        var previousDash = false;

        foreach (var character in normalized)
        {
            var next = character switch
            {
                >= 'a' and <= 'z' => character,
                >= '0' and <= '9' => character,
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                _ => '-'
            };

            if (next == '-')
            {
                if (previousDash)
                {
                    continue;
                }

                previousDash = true;
                buffer.Add(next);
                continue;
            }

            previousDash = false;
            buffer.Add(next);
        }

        return new string(buffer.ToArray()).Trim('-');
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static int NormalizeStock(int stock)
    {
        return Math.Max(0, stock);
    }

    private static decimal NormalizeMoney(decimal value)
    {
        return value < 0 ? 0 : value;
    }

    private static decimal? NormalizeMoney(decimal? value)
    {
        return value is null ? null : NormalizeMoney(value.Value);
    }

    private static decimal? NormalizeDecimal(decimal? value)
    {
        return value is null || value < 0 ? null : value;
    }

    private static decimal NormalizeRating(decimal value)
    {
        return Math.Clamp(value, 0m, 5m);
    }

    private static int NormalizeReviewCount(int value)
    {
        return Math.Max(0, value);
    }

    private static int? NormalizeCategoryId(int? categoryId)
    {
        return categoryId is > 0 ? categoryId : null;
    }

    private static decimal? NormalizeOldPrice(decimal? oldPrice)
    {
        return oldPrice is > 0 ? oldPrice : null;
    }

    private static decimal? NormalizeUnitAmount(bool showUnitPrice, decimal? value)
    {
        if (!showUnitPrice)
        {
            return null;
        }

        return value is > 0 ? value : null;
    }

    private static string? NormalizeUnitType(bool showUnitPrice, string? value)
    {
        if (!showUnitPrice)
        {
            return null;
        }

        return NormalizeOptionalText(value);
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

    private static ProductVariantFieldVisibilityViewModel ParseVariantFieldVisibility(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return new ProductVariantFieldVisibilityViewModel();
        }

        try
        {
            return JsonSerializer.Deserialize<ProductVariantFieldVisibilityViewModel>(rawJson) ?? new ProductVariantFieldVisibilityViewModel();
        }
        catch
        {
            return new ProductVariantFieldVisibilityViewModel();
        }
    }

    private static string SerializeVariantFieldVisibility(ProductVariantFieldVisibilityViewModel? visibility)
    {
        return JsonSerializer.Serialize(visibility ?? new ProductVariantFieldVisibilityViewModel());
    }

    private static IReadOnlyList<ProductVariantGroupInputViewModel> BuildVariantGroups(Product product)
    {
        if (product.ProductVariantGroups.Count > 0)
        {
            return product.ProductVariantGroups
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(x => new ProductVariantGroupInputViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    SelectionStyle = NormalizeSelectionStyle(x.SelectionStyle),
                    ShowOnCard = x.ShowOnCard,
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder,
                    Options = x.Options
                        .OrderBy(option => option.SortOrder)
                        .ThenBy(option => option.Id)
                        .Select(option => new ProductVariantOptionInputViewModel
                        {
                            Id = option.Id,
                            Name = option.Name,
                            ColorHex = option.ColorHex,
                            SwatchImageUrl = option.SwatchImageUrl,
                            SortOrder = option.SortOrder
                        })
                        .ToArray()
                })
                .ToArray();
        }

        var legacyGroups = product.ProductVariants
            .Where(x => !string.IsNullOrWhiteSpace(x.GroupName) && !string.IsNullOrWhiteSpace(x.OptionName))
            .GroupBy(x => x.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var nextTempId = -1;
        return legacyGroups
            .Select((group, groupIndex) => new ProductVariantGroupInputViewModel
            {
                Id = nextTempId--,
                Name = group.Key,
                SelectionStyle = "list",
                ShowOnCard = groupIndex == 0,
                IsPrimary = groupIndex == 0,
                SortOrder = groupIndex,
                Options = group
                    .Select(item => item.OptionName.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select((name, optionIndex) => new ProductVariantOptionInputViewModel
                    {
                        Id = nextTempId--,
                        Name = name,
                        SortOrder = optionIndex
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static IReadOnlyList<ProductVariantInputViewModel> BuildVariantInputs(Product product)
    {
        var groupInputs = BuildVariantGroups(product);
        var optionLookup = groupInputs
            .SelectMany(group => group.Options.Select(option => new
            {
                GroupName = group.Name,
                OptionName = option.Name,
                OptionId = option.Id ?? 0
            }))
            .ToDictionary(
                item => $"{item.GroupName}::{item.OptionName}",
                item => item.OptionId,
                StringComparer.OrdinalIgnoreCase);

        return product.ProductVariants
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.OptionName)
            .Select(x => new ProductVariantInputViewModel
            {
                Id = x.Id,
                DisplayName = string.IsNullOrWhiteSpace(x.DisplayName) ? x.OptionName : x.DisplayName,
                GroupName = x.GroupName,
                OptionName = x.OptionName,
                ImageUrl = x.ImageUrl,
                Sku = x.Sku,
                Barcode = x.Barcode,
                Price = x.Price,
                OldPrice = x.OldPrice,
                PurchasePrice = x.PurchasePrice,
                Stock = x.Stock,
                Desi = x.Desi,
                HsCode = x.HsCode,
                SortOrder = x.SortOrder,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                OptionIds = x.Selections.Count > 0
                    ? x.Selections
                        .OrderBy(selection => selection.ProductVariantOption?.ProductVariantGroup?.SortOrder ?? int.MaxValue)
                        .ThenBy(selection => selection.ProductVariantOption?.SortOrder ?? int.MaxValue)
                        .Select(selection => selection.ProductVariantOptionId)
                        .ToArray()
                    : BuildLegacyOptionIds(x, groupInputs, optionLookup)
            })
            .ToArray();
    }

    private static IReadOnlyList<ProductBundleItemInputViewModel> BuildBundleItems(Product product)
    {
        return product.ProductBundleItems
            .OrderBy(item => item.ProductVariantId ?? 0)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => new ProductBundleItemInputViewModel
            {
                Id = item.Id,
                ParentVariantId = item.ProductVariantId,
                ProductId = item.ChildProductId,
                ProductVariantId = item.ChildProductVariantId,
                EntryMode = NormalizeBundleEntryMode(item.EntryMode),
                ProductName = item.ChildProduct?.Name ?? string.Empty,
                ProductImageUrl = item.ChildProduct?.ImageUrl,
                ProductVariantLabel = item.ChildProductVariant is null
                    ? null
                    : string.IsNullOrWhiteSpace(item.ChildProductVariant.DisplayName)
                        ? item.ChildProductVariant.OptionName
                        : item.ChildProductVariant.DisplayName,
                UnitPrice = item.ChildProductVariant?.Price ?? item.ChildProduct?.Price ?? 0m,
                Quantity = item.Quantity,
                MinQuantity = item.MinQuantity,
                MaxQuantity = item.MaxQuantity,
                SortOrder = item.SortOrder
            })
            .ToArray();
    }

    private static IReadOnlyList<int> BuildLegacyOptionIds(
        ProductVariant variant,
        IReadOnlyList<ProductVariantGroupInputViewModel> groups,
        IReadOnlyDictionary<string, int> optionLookup)
    {
        if (!string.IsNullOrWhiteSpace(variant.GroupName) && !string.IsNullOrWhiteSpace(variant.OptionName))
        {
            var key = $"{variant.GroupName.Trim()}::{variant.OptionName.Trim()}";
            if (optionLookup.TryGetValue(key, out var singleOptionId))
            {
                return new[] { singleOptionId };
            }
        }

        var labels = (string.IsNullOrWhiteSpace(variant.DisplayName) ? variant.OptionName : variant.DisplayName)
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(label => label.Trim())
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .ToArray();

        if (labels.Length == 0)
        {
            return Array.Empty<int>();
        }

        var optionIds = new List<int>();
        foreach (var label in labels)
        {
            var match = groups
                .SelectMany(group => group.Options)
                .FirstOrDefault(option => string.Equals(option.Name, label, StringComparison.OrdinalIgnoreCase));

            if (match?.Id is int optionId)
            {
                optionIds.Add(optionId);
            }
        }

        return optionIds;
    }

    private static string NormalizeBundleEntryMode(string? entryMode)
    {
        return string.Equals(entryMode?.Trim(), "assortment", StringComparison.OrdinalIgnoreCase)
            ? "assortment"
            : "product";
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
                    AltText = linkedAsset?.AltText ?? (string.IsNullOrWhiteSpace(entity.Name) ? "�r�n gorseli" : entity.Name),
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

    private void ApplyProductCustomFields(Product entity, IReadOnlyList<int>? selectedCustomFieldIds)
    {
        var normalizedIds = selectedCustomFieldIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        var currentIds = entity.ProductCustomFields.Select(x => x.CustomFieldDefinitionId).ToList();
        foreach (var customFieldId in currentIds.Where(customFieldId => !normalizedIds.Contains(customFieldId)))
        {
            var relation = entity.ProductCustomFields.First(x => x.CustomFieldDefinitionId == customFieldId);
            entity.ProductCustomFields.Remove(relation);
        }

        foreach (var customFieldId in normalizedIds.Where(customFieldId => currentIds.All(existing => existing != customFieldId)))
        {
            entity.ProductCustomFields.Add(new ProductCustomField
            {
                ProductId = entity.Id,
                CustomFieldDefinitionId = customFieldId
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

    private static List<ProductVariantGroupInputViewModel> NormalizeVariantGroups(
        IReadOnlyList<ProductVariantGroupInputViewModel>? groups,
        IEnumerable<ProductVariant> variants)
    {
        var normalizedGroups = groups?
            .Where(group => !string.IsNullOrWhiteSpace(group.Name))
            .Select((group, groupIndex) => new ProductVariantGroupInputViewModel
            {
                Id = group.Id,
                Name = group.Name.Trim(),
                SelectionStyle = NormalizeSelectionStyle(group.SelectionStyle),
                ShowOnCard = group.ShowOnCard,
                IsPrimary = group.IsPrimary,
                SortOrder = group.SortOrder == 0 ? groupIndex : group.SortOrder,
                Options = group.Options
                    .Where(option => !string.IsNullOrWhiteSpace(option.Name))
                    .Select((option, optionIndex) => new ProductVariantOptionInputViewModel
                    {
                        Id = option.Id,
                        Name = option.Name.Trim(),
                        ColorHex = NormalizeOptionalText(option.ColorHex),
                        SwatchImageUrl = NormalizeOptionalText(option.SwatchImageUrl),
                        SortOrder = option.SortOrder == 0 ? optionIndex : option.SortOrder
                    })
                    .ToArray()
            })
            .Where(group => group.Options.Count > 0)
            .ToList() ?? new List<ProductVariantGroupInputViewModel>();

        if (normalizedGroups.Count == 0 && variants.Any())
        {
            normalizedGroups = BuildLegacyVariantGroupsFromVariants(variants).ToList();
        }

        EnsurePrimaryGroup(normalizedGroups);
        return normalizedGroups;
    }

    private static IReadOnlyList<ProductVariantGroupInputViewModel> BuildLegacyVariantGroupsFromVariants(IEnumerable<ProductVariant> variants)
    {
        var nextTempId = -1;
        return variants
            .Where(variant => !string.IsNullOrWhiteSpace(variant.GroupName) && !string.IsNullOrWhiteSpace(variant.OptionName))
            .GroupBy(variant => variant.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select((group, groupIndex) => new ProductVariantGroupInputViewModel
            {
                Id = nextTempId--,
                Name = group.Key,
                SelectionStyle = "list",
                ShowOnCard = groupIndex == 0,
                IsPrimary = groupIndex == 0,
                SortOrder = groupIndex,
                Options = group
                    .Select(item => item.OptionName.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select((name, optionIndex) => new ProductVariantOptionInputViewModel
                    {
                        Id = nextTempId--,
                        Name = name,
                        SortOrder = optionIndex
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static void EnsurePrimaryGroup(List<ProductVariantGroupInputViewModel> groups)
    {
        if (groups.Count == 0)
        {
            return;
        }

        var primaryGroup = groups.FirstOrDefault(group => group.IsPrimary) ?? groups[0];
        foreach (var group in groups)
        {
            group.IsPrimary = ReferenceEquals(group, primaryGroup);
        }
    }

    private static void EnsurePrimaryVariant(List<ProductVariantInputViewModel> variants)
    {
        if (variants.Count == 0)
        {
            return;
        }

        var primaryVariant = variants.FirstOrDefault(variant => variant.IsDefault) ?? variants[0];
        foreach (var variant in variants)
        {
            variant.IsDefault = ReferenceEquals(variant, primaryVariant);
        }
    }

    private static Dictionary<int, ProductVariantOption> SyncVariantGroups(Product entity, IReadOnlyList<ProductVariantGroupInputViewModel> groups)
    {
        var optionLookup = new Dictionary<int, ProductVariantOption>();
        var incomingIds = groups
            .Where(group => group.Id.HasValue && group.Id.Value > 0)
            .Select(group => group.Id!.Value)
            .ToHashSet();

        var removableGroups = entity.ProductVariantGroups
            .Where(group => !incomingIds.Contains(group.Id))
            .ToList();

        foreach (var removableGroup in removableGroups)
        {
            entity.ProductVariantGroups.Remove(removableGroup);
        }

        foreach (var item in groups)
        {
            var existingGroup = item.Id.HasValue && item.Id.Value > 0
                ? entity.ProductVariantGroups.FirstOrDefault(group => group.Id == item.Id.Value)
                : null;

            if (existingGroup is null)
            {
                existingGroup = new ProductVariantGroup();
                entity.ProductVariantGroups.Add(existingGroup);
            }

            existingGroup.Name = item.Name;
            existingGroup.SelectionStyle = NormalizeSelectionStyle(item.SelectionStyle);
            existingGroup.ShowOnCard = item.ShowOnCard;
            existingGroup.IsPrimary = item.IsPrimary;
            existingGroup.SortOrder = item.SortOrder;
            existingGroup.UpdatedAt = DateTime.UtcNow;

            SyncVariantOptions(existingGroup, item.Options, optionLookup);
        }

        return optionLookup;
    }

    private static void SyncVariantOptions(
        ProductVariantGroup groupEntity,
        IReadOnlyList<ProductVariantOptionInputViewModel> options,
        Dictionary<int, ProductVariantOption> optionLookup)
    {
        var incomingIds = options
            .Where(option => option.Id.HasValue && option.Id.Value > 0)
            .Select(option => option.Id!.Value)
            .ToHashSet();

        var removableOptions = groupEntity.Options
            .Where(option => !incomingIds.Contains(option.Id))
            .ToList();

        foreach (var removableOption in removableOptions)
        {
            groupEntity.Options.Remove(removableOption);
        }

        foreach (var option in options)
        {
            var existingOption = option.Id.HasValue && option.Id.Value > 0
                ? groupEntity.Options.FirstOrDefault(item => item.Id == option.Id.Value)
                : null;

            if (existingOption is null)
            {
                existingOption = new ProductVariantOption();
                groupEntity.Options.Add(existingOption);
            }

            existingOption.Name = option.Name;
            existingOption.ColorHex = NormalizeOptionalText(option.ColorHex);
            existingOption.SwatchImageUrl = NormalizeOptionalText(option.SwatchImageUrl);
            existingOption.SortOrder = option.SortOrder;
            existingOption.UpdatedAt = DateTime.UtcNow;

            if (option.Id.HasValue)
            {
                optionLookup[option.Id.Value] = existingOption;
            }
        }
    }

    private static string ResolveVariantDisplayName(ProductVariantInputViewModel item, IReadOnlyList<ProductVariantOption> selectedOptions)
    {
        if (!string.IsNullOrWhiteSpace(item.DisplayName))
        {
            return item.DisplayName.Trim();
        }

        return selectedOptions.Count == 0
            ? string.Empty
            : string.Join(" / ", selectedOptions.OrderBy(GetOptionSortKey).Select(option => option.Name));
    }

    private static string ResolveVariantGroupSummary(IReadOnlyList<ProductVariantOption> selectedOptions)
    {
        return string.Join(" / ",
            selectedOptions
                .Select(option => option.ProductVariantGroup?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)!);
    }

    private static string ResolveVariantOptionSummary(IReadOnlyList<ProductVariantOption> selectedOptions, string displayName)
    {
        return selectedOptions.Count == 0
            ? displayName
            : string.Join(" / ", selectedOptions.OrderBy(GetOptionSortKey).Select(option => option.Name));
    }

    private static void SyncVariantSelections(ProductVariant variantEntity, IReadOnlyList<ProductVariantOption> selectedOptions)
    {
        var selectedIds = selectedOptions.Select(option => option.Id).Where(id => id > 0).ToHashSet();
        var removableSelections = variantEntity.Selections
            .Where(selection => selection.ProductVariantOptionId > 0 && !selectedIds.Contains(selection.ProductVariantOptionId))
            .ToList();

        foreach (var removableSelection in removableSelections)
        {
            variantEntity.Selections.Remove(removableSelection);
        }

        foreach (var option in selectedOptions)
        {
            if (variantEntity.Selections.Any(selection =>
                (option.Id > 0 && selection.ProductVariantOptionId == option.Id) ||
                ReferenceEquals(selection.ProductVariantOption, option)))
            {
                continue;
            }

            variantEntity.Selections.Add(new ProductVariantSelection
            {
                ProductVariantOptionId = option.Id,
                ProductVariantOption = option
            });
        }
    }

    private static int GetOptionSortKey(ProductVariantOption option)
    {
        return (option.ProductVariantGroup?.SortOrder ?? int.MaxValue) * 10000 + option.SortOrder;
    }

    private void ApplyProductPersonalizations(Product entity, IReadOnlyList<int>? selectedPersonalizationIds)
    {
        var normalizedIds = selectedPersonalizationIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        var currentIds = entity.ProductPersonalizations.Select(x => x.PersonalizationDefinitionId).ToList();
        foreach (var personalizationId in currentIds.Where(personalizationId => !normalizedIds.Contains(personalizationId)))
        {
            var relation = entity.ProductPersonalizations.First(x => x.PersonalizationDefinitionId == personalizationId);
            entity.ProductPersonalizations.Remove(relation);
        }

        foreach (var personalizationId in normalizedIds.Where(personalizationId => currentIds.All(existing => existing != personalizationId)))
        {
            entity.ProductPersonalizations.Add(new ProductPersonalization
            {
                ProductId = entity.Id,
                PersonalizationDefinitionId = personalizationId
            });
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

    private static void ApplyProductCategories(Product entity, int? primaryCategoryId, IReadOnlyList<int>? selectedCategoryIds)
    {
        var normalizedCategoryIds = selectedCategoryIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet() ?? new HashSet<int>();

        if (primaryCategoryId is > 0)
        {
            normalizedCategoryIds.Add(primaryCategoryId.Value);
        }

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

    private static string NormalizeSelectionStyle(string? style)
    {
        return string.Equals(style?.Trim(), "visual", StringComparison.OrdinalIgnoreCase)
            ? "visual"
            : "list";
    }

    private static DateTime ResolveUpdatedAt(Product product)
    {
        var latestVariantUpdate = product.ProductVariants.Count == 0
            ? (DateTime?)null
            : product.ProductVariants.Max(x => x.UpdatedAt);

        var latestMediaUpdate = product.ProductMedias.Count == 0
            ? (DateTime?)null
            : product.ProductMedias.Max(x => x.UpdatedAt);

        return new[]
        {
            product.CreatedAt,
            latestVariantUpdate ?? product.CreatedAt,
            latestMediaUpdate ?? product.CreatedAt
        }.Max();
    }

    private static void ApplyProductVariants(
        Product entity,
        IReadOnlyList<ProductVariantInputViewModel>? variants,
        IReadOnlyList<ProductVariantGroupInputViewModel>? groups)
    {
        var normalizedGroups = NormalizeVariantGroups(groups, entity.ProductVariants);
        var optionEntityLookup = SyncVariantGroups(entity, normalizedGroups);

        var normalizedVariants = variants?
            .Where(x => x.OptionIds.Count > 0)
            .Select((x, index) => new ProductVariantInputViewModel
            {
                Id = x.Id,
                DisplayName = NormalizeFreeText(x.DisplayName),
                GroupName = NormalizeFreeText(x.GroupName),
                OptionName = NormalizeFreeText(x.OptionName),
                ImageUrl = NormalizeOptionalText(x.ImageUrl),
                Sku = string.IsNullOrWhiteSpace(x.Sku) ? null : x.Sku.Trim(),
                Barcode = NormalizeOptionalText(x.Barcode),
                Price = x.Price,
                OldPrice = NormalizeOldPrice(x.OldPrice),
                PurchasePrice = NormalizeMoney(x.PurchasePrice),
                Stock = x.Stock,
                Desi = NormalizeDecimal(x.Desi),
                HsCode = NormalizeOptionalText(x.HsCode),
                SortOrder = x.SortOrder == 0 ? index : x.SortOrder,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                OptionIds = x.OptionIds
                    .Distinct()
                    .Where(id => id != 0)
                    .ToArray()
            })
            .Where(x => x.OptionIds.Count > 0)
            .ToList() ?? new List<ProductVariantInputViewModel>();

        EnsurePrimaryVariant(normalizedVariants);

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
                existing = new ProductVariant
                {
                    UpdatedAt = DateTime.UtcNow
                };
                entity.ProductVariants.Add(existing);
            }

            var selectedOptionEntities = item.OptionIds
                .Select(id => optionEntityLookup.TryGetValue(id, out var optionEntity) ? optionEntity : null)
                .Where(option => option is not null)
                .Cast<ProductVariantOption>()
                .ToList();

            var resolvedDisplayName = ResolveVariantDisplayName(item, selectedOptionEntities);
            existing.DisplayName = resolvedDisplayName;
            existing.GroupName = ResolveVariantGroupSummary(selectedOptionEntities);
            existing.OptionName = ResolveVariantOptionSummary(selectedOptionEntities, resolvedDisplayName);
            existing.ImageUrl = item.ImageUrl;
            existing.Sku = item.Sku;
            existing.Barcode = item.Barcode;
            existing.Price = item.Price;
            existing.OldPrice = item.OldPrice;
            existing.PurchasePrice = item.PurchasePrice;
            existing.Stock = item.Stock;
            existing.Desi = item.Desi;
            existing.HsCode = item.HsCode;
            existing.SortOrder = item.SortOrder;
            existing.IsDefault = item.IsDefault;
            existing.IsActive = item.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            SyncVariantSelections(existing, selectedOptionEntities);
        }
    }

    private void ApplyBundleItems(Product entity, IReadOnlyList<ProductBundleItemInputViewModel>? items)
    {
        var normalizedItems = items?
            .Where(item => item.ProductId > 0)
            .Select((item, index) => new ProductBundleItemInputViewModel
            {
                Id = item.Id,
                ParentVariantId = item.ParentVariantId,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                EntryMode = NormalizeBundleEntryMode(item.EntryMode),
                ProductName = NormalizeFreeText(item.ProductName),
                ProductImageUrl = NormalizeOptionalText(item.ProductImageUrl),
                ProductVariantLabel = NormalizeOptionalText(item.ProductVariantLabel),
                UnitPrice = NormalizeMoney(item.UnitPrice),
                Quantity = Math.Max(0, item.Quantity),
                MinQuantity = item.MinQuantity is >= 0 ? item.MinQuantity : null,
                MaxQuantity = item.MaxQuantity is >= 0 ? item.MaxQuantity : null,
                SortOrder = item.SortOrder == 0 ? index : item.SortOrder
            })
            .ToList() ?? new List<ProductBundleItemInputViewModel>();

        var childProductIds = normalizedItems
            .Select(item => item.ProductId)
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        var childVariantIds = normalizedItems
            .Select(item => item.ProductVariantId ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        var childProducts = _dbContext.Products
            .Where(product => childProductIds.Contains(product.Id))
            .ToDictionary(product => product.Id);
        var childVariants = _dbContext.ProductVariants
            .Where(variant => childVariantIds.Contains(variant.Id))
            .ToDictionary(variant => variant.Id);

        var incomingIds = normalizedItems
            .Where(item => item.Id.HasValue && item.Id.Value > 0)
            .Select(item => item.Id!.Value)
            .ToHashSet();

        var removableItems = entity.ProductBundleItems
            .Where(item => !incomingIds.Contains(item.Id))
            .ToList();

        foreach (var removableItem in removableItems)
        {
            entity.ProductBundleItems.Remove(removableItem);
        }

        foreach (var item in normalizedItems)
        {
            var existing = item.Id.HasValue && item.Id.Value > 0
                ? entity.ProductBundleItems.FirstOrDefault(bundleItem => bundleItem.Id == item.Id.Value)
                : null;

            if (existing is null)
            {
                existing = new ProductBundleItem();
                entity.ProductBundleItems.Add(existing);
            }

            existing.ProductVariantId = item.ParentVariantId is > 0 ? item.ParentVariantId : null;
            existing.ChildProductId = item.ProductId;
            existing.ChildProductVariantId = item.ProductVariantId is > 0 ? item.ProductVariantId : null;
            existing.ChildProduct = childProducts.GetValueOrDefault(item.ProductId);
            existing.ChildProductVariant = item.ProductVariantId is > 0
                ? childVariants.GetValueOrDefault(item.ProductVariantId.Value)
                : null;
            existing.EntryMode = NormalizeBundleEntryMode(item.EntryMode);
            existing.Quantity = Math.Max(0, item.Quantity);
            existing.MinQuantity = item.MinQuantity is >= 0 ? item.MinQuantity : null;
            existing.MaxQuantity = item.MaxQuantity is >= 0 ? item.MaxQuantity : null;
            existing.SortOrder = item.SortOrder;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void ApplyBundleSummary(Product entity, ProductFormViewModel model)
    {
        if (entity.ProductKind != ProductKind.Bundle)
        {
            entity.BundleMode = null;
            entity.BundlePricingMode = null;
            entity.BundleAdjustmentType = null;
            entity.BundleAdjustmentAmount = null;
            entity.BundleTotalQuantity = null;
            return;
        }

        var bundleMode = NormalizeBundleMode(model.BundleMode, model.CreateMode == "bundle-variant");
        if (bundleMode == "variant")
        {
            ApplyVariantBundleSummary(entity, model);
            return;
        }

        var simpleItems = entity.ProductBundleItems
            .Where(item => item.ProductVariantId is null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        if (NormalizeBundlePricingMode(model.BundlePricingMode) == "sum")
        {
            entity.Price = CalculateBundleItemsPrice(simpleItems, entity.BundleAdjustmentType, entity.BundleAdjustmentAmount);
            entity.OldPrice = null;
        }

        entity.Stock = CalculateBundleItemsStock(simpleItems, entity);
    }

    private static void ApplyVariantBundleSummary(Product entity, ProductFormViewModel model)
    {
        var pricingMode = NormalizeBundlePricingMode(model.BundlePricingMode);
        foreach (var variant in entity.ProductVariants)
        {
            var variantItems = entity.ProductBundleItems
                .Where(item => item.ProductVariantId == variant.Id)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToList();

            if (pricingMode == "sum")
            {
                variant.Price = CalculateBundleItemsPrice(variantItems, entity.BundleAdjustmentType, entity.BundleAdjustmentAmount);
                variant.OldPrice = null;
            }

            variant.Stock = CalculateBundleItemsStock(variantItems, entity);
        }
    }

    private static decimal CalculateBundleItemsPrice(
        IReadOnlyCollection<ProductBundleItem> items,
        string? adjustmentType,
        decimal? adjustmentAmount)
    {
        var total = items.Sum(item =>
        {
            var unitPrice = item.ChildProductVariant?.Price ?? item.ChildProduct?.Price ?? 0m;
            return unitPrice * Math.Max(0, item.Quantity);
        });

        return NormalizeBundleAdjustmentType(adjustmentType) switch
        {
            "increase" => total + (adjustmentAmount ?? 0m),
            "discount" => Math.Max(0m, total - (adjustmentAmount ?? 0m)),
            _ => total
        };
    }

    private static int CalculateBundleItemsStock(IReadOnlyCollection<ProductBundleItem> items, Product entity)
    {
        var requiredItems = items.Where(item => item.Quantity > 0).ToList();
        if (requiredItems.Count == 0)
        {
            return entity.Stock;
        }

        return requiredItems
            .Select(item =>
            {
                var sourceStock = item.ChildProductVariant?.Stock ?? item.ChildProduct?.Stock ?? 0;
                return sourceStock / Math.Max(1, item.Quantity);
            })
            .Min();
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
            if (entity.ProductKind == ProductKind.Bundle)
            {
                if (NormalizeBundlePricingMode(model.BundlePricingMode) == "manual")
                {
                    entity.Price = NormalizeMoney(model.Price);
                    entity.OldPrice = NormalizeOldPrice(model.OldPrice);
                }
                return;
            }

            entity.Price = NormalizeMoney(model.Price);
            entity.OldPrice = NormalizeOldPrice(model.OldPrice);
            entity.Stock = NormalizeStock(model.Stock);
            return;
        }

        var primaryVariant = activeVariants[0];
        entity.Price = primaryVariant.Price;
        entity.OldPrice = primaryVariant.OldPrice;
        entity.Stock = activeVariants.Sum(x => x.Stock);
    }
}
