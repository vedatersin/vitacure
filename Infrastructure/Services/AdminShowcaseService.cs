using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminShowcaseService : IAdminShowcaseService
{
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ISlugService _slugService;
    private static readonly HashSet<string> AllowedBackgroundExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    };

    public AdminShowcaseService(AppDbContext dbContext, ICacheInvalidationService cacheInvalidationService, IWebHostEnvironment environment, ISlugService slugService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _environment = environment;
        _slugService = slugService;
    }

    public async Task<ShowcaseListViewModel> GetShowcasesAsync(CancellationToken cancellationToken = default)
    {
        var showcases = await _dbContext.Showcases
            .AsNoTracking()
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = showcases.Select(showcase => new ShowcaseListItemViewModel
        {
            Id = showcase.Id,
            Name = showcase.Name,
            Title = showcase.Title,
            Slug = showcase.Slug,
            IconClass = showcase.IconClass,
            BackgroundImageUrl = showcase.BackgroundImageUrl,
            IsActive = showcase.IsActive,
            ShowOnHome = showcase.ShowOnHome,
            CategoryCount = showcase.ShowcaseCategories.Count,
            FeaturedProductCount = showcase.FeaturedProducts.Count
        }).ToList();

        return new ShowcaseListViewModel
        {
            TotalCount = items.Count,
            ActiveCount = items.Count(x => x.IsActive),
            HomeVisibleCount = items.Count(x => x.ShowOnHome),
            Showcases = items
        };
    }

    public async Task<ShowcaseFormViewModel> GetCreateModelAsync(CancellationToken cancellationToken = default)
    {
        var productOptions = await GetProductOptionsAsync(cancellationToken);

        return new ShowcaseFormViewModel
        {
            IconClass = "fa-solid fa-sparkles",
            IsDark = true,
            BackgroundImageUrl = GetRecommendedBackgroundImageUrl(null, null),
            BackgroundOptions = GetBackgroundOptions(null, null),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            ProductOptions = productOptions,
            SelectedFeaturedProductIds = BuildInitialFeaturedProductIds(productOptions)
        };
    }

    public async Task<ShowcaseFormViewModel?> GetEditModelAsync(int id, CancellationToken cancellationToken = default)
    {
        var showcase = await _dbContext.Showcases
            .AsNoTracking()
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (showcase is null)
        {
            return null;
        }

        return new ShowcaseFormViewModel
        {
            Id = showcase.Id,
            Name = showcase.Name,
            Slug = showcase.Slug,
            IconClass = string.IsNullOrWhiteSpace(showcase.IconClass) ? "fa-solid fa-sparkles" : showcase.IconClass,
            Title = showcase.Title,
            Description = showcase.Description,
            TagsContent = showcase.TagsContent,
            BackgroundImageUrl = showcase.BackgroundImageUrl,
            IsDark = showcase.IsDark,
            SeoTitle = showcase.SeoTitle,
            MetaDescription = showcase.MetaDescription,
            ShowOnHome = showcase.ShowOnHome,
            IsActive = showcase.IsActive,
            SortOrder = showcase.SortOrder,
            BackgroundOptions = GetBackgroundOptions(showcase.Name, showcase.Slug),
            SelectedCategoryIds = showcase.ShowcaseCategories.Select(x => x.CategoryId).ToList(),
            SelectedFeaturedProductIds = showcase.FeaturedProducts.OrderBy(x => x.SortOrder).Select(x => x.ProductId).ToList(),
            CategoryOptions = await GetCategoryOptionsAsync(cancellationToken),
            ProductOptions = await GetProductOptionsAsync(cancellationToken)
        };
    }

    public async Task<int> CreateAsync(ShowcaseFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Showcase, cancellationToken: cancellationToken);

        var entity = new Showcase
        {
            Name = model.Name.Trim(),
            Slug = normalizedSlug,
            IconClass = CleanIconClass(model.IconClass),
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            TagsContent = NormalizeMultiline(model.TagsContent),
            BackgroundImageUrl = await ResolveBackgroundImageAsync(model, cancellationToken),
            IsDark = model.IsDark,
            SeoTitle = Clean(model.SeoTitle),
            MetaDescription = Clean(model.MetaDescription),
            ShowOnHome = model.ShowOnHome,
            IsActive = model.IsActive,
            SortOrder = model.SortOrder,
            UpdatedAt = DateTime.UtcNow
        };

        entity.ShowcaseCategories = model.SelectedCategoryIds
            .Distinct()
            .Select(categoryId => new ShowcaseCategory
            {
                CategoryId = categoryId
            })
            .ToList();

        entity.FeaturedProducts = model.SelectedFeaturedProductIds
            .Take(7)
            .Distinct()
            .Select((productId, index) => new ShowcaseFeaturedProduct
            {
                ProductId = productId,
                SortOrder = index
            })
            .ToList();

        _dbContext.Showcases.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateStorefrontAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ShowcaseFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Id is null)
        {
            return false;
        }

        var entity = await _dbContext.Showcases
            .Include(x => x.ShowcaseCategories)
            .Include(x => x.FeaturedProducts)
            .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        var normalizedSlug = model.Slug.Trim();
        await _slugService.EnsureAvailableAsync(normalizedSlug, SlugEntityType.Showcase, entity.Id, cancellationToken);

        entity.Name = model.Name.Trim();
        entity.Slug = normalizedSlug;
        entity.IconClass = CleanIconClass(model.IconClass);
        entity.Title = model.Title.Trim();
        entity.Description = model.Description.Trim();
        entity.TagsContent = NormalizeMultiline(model.TagsContent);
        entity.BackgroundImageUrl = await ResolveBackgroundImageAsync(model, cancellationToken);
        entity.IsDark = model.IsDark;
        entity.SeoTitle = Clean(model.SeoTitle);
        entity.MetaDescription = Clean(model.MetaDescription);
        entity.ShowOnHome = model.ShowOnHome;
        entity.IsActive = model.IsActive;
        entity.SortOrder = model.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        var selectedCategoryIds = model.SelectedCategoryIds.Distinct().ToArray();
        if (selectedCategoryIds.Length == 0 && entity.ShowcaseCategories.Count > 0)
        {
            selectedCategoryIds = entity.ShowcaseCategories
                .Select(x => x.CategoryId)
                .Distinct()
                .ToArray();
        }

        var selectedFeaturedProductIds = model.SelectedFeaturedProductIds
            .Take(7)
            .Distinct()
            .ToArray();
        if (selectedFeaturedProductIds.Length == 0 && entity.FeaturedProducts.Count > 0)
        {
            selectedFeaturedProductIds = entity.FeaturedProducts
                .OrderBy(x => x.SortOrder)
                .Select(x => x.ProductId)
                .Distinct()
                .Take(7)
                .ToArray();
        }

        entity.ShowcaseCategories.Clear();
        foreach (var categoryId in selectedCategoryIds)
        {
            entity.ShowcaseCategories.Add(new ShowcaseCategory
            {
                ShowcaseId = entity.Id,
                CategoryId = categoryId
            });
        }

        entity.FeaturedProducts.Clear();
        foreach (var item in selectedFeaturedProductIds.Select((productId, index) => new { productId, index }))
        {
            entity.FeaturedProducts.Add(new ShowcaseFeaturedProduct
            {
                ShowcaseId = entity.Id,
                ProductId = item.productId,
                SortOrder = item.index
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateStorefrontAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<ShowcaseCategoryOptionViewModel>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Include(x => x.Parent)
            .OrderBy(x => x.ParentId.HasValue)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(category => new ShowcaseCategoryOptionViewModel
        {
            Id = category.Id,
            Name = category.Parent is null ? category.Name : $"{category.Parent.Name} / {category.Name}"
        }).ToList();
    }

    private async Task<IReadOnlyList<ShowcaseProductOptionViewModel>> GetProductOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ShowcaseProductOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                CategoryName = x.Category != null ? x.Category.Name : "-",
                CategorySlug = x.Category != null ? x.Category.Slug : string.Empty,
                CategoryId = x.CategoryId,
                ImageUrl = x.ImageUrl,
                TagNames = x.ProductTags
                    .Where(tag => tag.Tag != null)
                    .Select(tag => tag.Tag!.Name)
                    .Distinct()
                    .OrderBy(tag => tag)
                    .ToArray()
            })
            .ToListAsync(cancellationToken);
    }

    private static List<int> BuildInitialFeaturedProductIds(IReadOnlyList<ShowcaseProductOptionViewModel> productOptions)
    {
        return productOptions
            .OrderBy(_ => Guid.NewGuid())
            .Take(7)
            .Select(product => product.Id)
            .ToList();
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string CleanIconClass(string? value)
        => string.IsNullOrWhiteSpace(value) ? "fa-solid fa-sparkles" : value.Trim();

    private static string NormalizeMultiline(string value)
        => string.Join(Environment.NewLine, value
            .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private async Task<string> ResolveBackgroundImageAsync(ShowcaseFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.BackgroundImageFile is { Length: > 0 })
        {
            var uploadedPath = await SaveBackgroundImageAsync(model.BackgroundImageFile, model.Slug, cancellationToken);
            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                return uploadedPath;
            }
        }

        var currentValue = model.BackgroundImageUrl;
        var name = model.Name;
        var slug = model.Slug;

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return GetRecommendedBackgroundImageUrl(name, slug);
        }

        var normalizedValue = currentValue.Trim();
        return IsValidBackgroundAsset(normalizedValue)
            ? normalizedValue
            : GetRecommendedBackgroundImageUrl(name, slug);
    }

    private async Task<string?> SaveBackgroundImageAsync(IFormFile file, string? slug, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedBackgroundExtensions.Contains(extension))
        {
            return null;
        }

        var uploadsDirectory = Path.Combine(_environment.WebRootPath, "img", "showcases");
        Directory.CreateDirectory(uploadsDirectory);

        var safeSlug = string.IsNullOrWhiteSpace(slug)
            ? "showcase"
            : NormalizeFileSegment(slug);
        var fileName = $"{safeSlug}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/img/showcases/{fileName}";
    }

    private IReadOnlyList<ShowcaseBackgroundOptionViewModel> GetBackgroundOptions(string? name, string? slug)
    {
        var recommended = GetRecommendedBackgroundImageUrl(name, slug);
        var imageRoot = Path.Combine(_environment.WebRootPath, "img");
        if (!Directory.Exists(imageRoot))
        {
            return Array.Empty<ShowcaseBackgroundOptionViewModel>();
        }

        return GetBackgroundAssetPaths(imageRoot)
            .Select(path => $"/img/{Path.GetFileName(path)}")
            .OrderBy(path => path)
            .Select(path => new ShowcaseBackgroundOptionViewModel
            {
                Name = Path.GetFileNameWithoutExtension(path.TrimStart('/').Split('/').Last()),
                ImageUrl = path,
                IsRecommended = string.Equals(path, recommended, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();
    }

    private string GetRecommendedBackgroundImageUrl(string? name, string? slug)
    {
        var imageRoot = Path.Combine(_environment.WebRootPath, "img");
        if (!Directory.Exists(imageRoot))
        {
            return string.Empty;
        }

        var explicitMatch = GetExplicitBackgroundImage(slug);
        if (!string.IsNullOrWhiteSpace(explicitMatch))
        {
            return explicitMatch;
        }

        var candidates = GetBackgroundAssetPaths(imageRoot)
            .Select(path => new
            {
                Path = $"/img/{Path.GetFileName(path)}",
                Normalized = NormalizeForMatch(Path.GetFileNameWithoutExtension(path))
            })
            .ToList();

        var matchTerms = new[]
        {
            NormalizeForMatch(name),
            NormalizeForMatch(slug)
        }.Where(term => !string.IsNullOrWhiteSpace(term)).ToArray();

        foreach (var term in matchTerms)
        {
            var exact = candidates.FirstOrDefault(item => item.Normalized.Contains(term, StringComparison.OrdinalIgnoreCase) || term.Contains(item.Normalized, StringComparison.OrdinalIgnoreCase));
            if (exact is not null)
            {
                return exact.Path;
            }
        }

        return candidates.FirstOrDefault()?.Path ?? string.Empty;
    }

    private static string GetExplicitBackgroundImage(string? slug)
    {
        return slug?.Trim().ToLowerInvariant() switch
        {
            "uyku-sagligi" or "uyku-rutini" => "/img/uykuBg.png",
            "multivitamin-enerji" or "multivitamin-enerji-plani" => "/img/multivitaminBg.png",
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "/img/zekaHafızaBg.png",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "/img/hastalıkKorumaBg.png",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "/img/kasİskeletBg.png",
            "zayiflama-destegi" or "zayiflama-rotasi" => "/img/zayıflamaBg.png",
            _ => string.Empty
        };
    }

    private static IReadOnlyList<string> GetBackgroundAssetPaths(string imageRoot)
    {
        return Directory.GetFiles(imageRoot, "*Bg.png", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                return fileName.EndsWith("Bg", StringComparison.OrdinalIgnoreCase)
                    && !fileName.Contains("nobg", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    private static bool IsValidBackgroundAsset(string value)
    {
        var fileName = Path.GetFileNameWithoutExtension(value.Trim());
        var extension = Path.GetExtension(value.Trim());
        return AllowedBackgroundExtensions.Contains(extension)
            && (!string.IsNullOrWhiteSpace(fileName) && !fileName.Contains("nobg", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeFileSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => invalidChars.Contains(character) ? '-' : character)
            .ToArray());

        return sanitized
            .Replace(" ", "-")
            .Replace("--", "-");
    }

    private static string NormalizeForMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("İ", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("&", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("destegi", string.Empty)
            .Replace("sagligi", string.Empty)
            .Replace("guclendirme", string.Empty)
            .Replace("karsi", string.Empty);
    }
}
