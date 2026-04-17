using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public class StorefrontContentService : IStorefrontContentService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IHomeContentConfigurationService _homeContentConfigurationService;
    private readonly IProductService _productService;
    private readonly Lazy<StorefrontUiDocument> _document;

    public StorefrontContentService(
        AppDbContext dbContext,
        IWebHostEnvironment environment,
        IProductService productService,
        IHomeContentConfigurationService homeContentConfigurationService)
    {
        _dbContext = dbContext;
        _environment = environment;
        _homeContentConfigurationService = homeContentConfigurationService;
        _productService = productService;
        _document = new Lazy<StorefrontUiDocument>(LoadDocument, true);
    }

    public async Task<HomeViewModel> GetHomePageContentAsync(CancellationToken cancellationToken = default)
    {
        var document = _document.Value;
        var homeConfiguration = await _homeContentConfigurationService.GetConfigurationAsync(cancellationToken);
        var categories = await BuildCategoriesAsync(cancellationToken);
        var showcases = await BuildHomeShowcasesAsync(cancellationToken);
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var featuredProducts = products.Take(12).ToList();
        var opportunityProducts = products
            .OrderByDescending(x => (x.OldPrice ?? x.Price) - x.Price)
            .ThenByDescending(x => x.Rating)
            .Take(12)
            .ToList();

        return new HomeViewModel
        {
            Title = "Ana Sayfa",
            MetaDescription = homeConfiguration.MetaDescription,
            CanonicalPath = "/",
            Showcases = showcases,
            Categories = categories,
            ChatWidget = BuildChatWidget(document, categories, showcases, null, "home", homeConfiguration.Chat),
            FeaturedProducts = BuildProductCards(featuredProducts),
            PopularSupplements = homeConfiguration.PopularSupplements,
            FeaturedBanner = homeConfiguration.FeaturedBanner,
            CampaignBanners = homeConfiguration.CampaignBanners,
            OpportunityProducts = BuildProductCards(opportunityProducts),
            SectionTitles = homeConfiguration.SectionHeaders.ToDictionary(x => x.Key, x => x.Value.Title, StringComparer.OrdinalIgnoreCase),
            SectionHeaders = homeConfiguration.SectionHeaders
        };
    }

    public async Task<ShowcaseViewModel?> GetShowcasePageContentAsync(string slug, string? categorySlug = null, CancellationToken cancellationToken = default)
    {
        var document = _document.Value;
        var showcase = await _dbContext.Showcases
            .AsNoTracking()
            .Include(x => x.ShowcaseCategories)
            .ThenInclude(x => x.Category)
            .Include(x => x.FeaturedProducts)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x!.Category)
            .FirstOrDefaultAsync(x => x.IsActive && x.Slug == slug, cancellationToken);

        if (showcase is null)
        {
            return null;
        }

        var categories = await BuildCategoriesAsync(cancellationToken);
        var selectedCategoryIds = showcase.ShowcaseCategories
            .Select(x => x.CategoryId)
            .Distinct()
            .ToArray();
        if (selectedCategoryIds.Length == 0)
        {
            var fallbackCategorySlug = ResolveShowcaseCategorySlug(showcase.Slug);
            if (!string.IsNullOrWhiteSpace(fallbackCategorySlug))
            {
                selectedCategoryIds = await _dbContext.Categories
                    .AsNoTracking()
                    .Where(x => x.IsActive && x.Slug == fallbackCategorySlug)
                    .Select(x => x.Id)
                    .Take(1)
                    .ToArrayAsync(cancellationToken);
            }
        }
        var selectedCategories = showcase.ShowcaseCategories
            .Where(x => x.Category is not null && x.Category.IsActive)
            .Select(x => x.Category!)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.ParentId.HasValue)
            .ThenBy(x => x.Name)
            .ToList();
        if (selectedCategories.Count == 0)
        {
            var fallbackCategorySlug = ResolveShowcaseCategorySlug(showcase.Slug);
            if (!string.IsNullOrWhiteSpace(fallbackCategorySlug))
            {
                selectedCategories = await _dbContext.Categories
                    .AsNoTracking()
                    .Where(x => x.IsActive && x.Slug == fallbackCategorySlug)
                    .OrderBy(x => x.ParentId.HasValue)
                    .ThenBy(x => x.Name)
                    .ToListAsync(cancellationToken);
            }
        }

        var productQuery = _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsActive && selectedCategoryIds.Contains(x.CategoryId));

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            productQuery = productQuery.Where(x => x.Category != null && x.Category.Slug == categorySlug);
        }

        var poolProducts = await productQuery
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var featuredProducts = showcase.FeaturedProducts
            .Where(x => x.Product is not null && x.Product.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Product!)
            .ToList();

        if (featuredProducts.Count == 0)
        {
            featuredProducts = poolProducts.Take(7).ToList();
        }

        var coverflowProducts = BuildCoverflowProducts(featuredProducts);
        var categoryOptions = selectedCategories
            .Select(category => new CategoryTagViewModel
            {
                Label = category.Name,
                Slug = category.Slug,
                Url = string.Equals(categorySlug, category.Slug, StringComparison.OrdinalIgnoreCase)
                    ? $"/{showcase.Slug}"
                    : $"/{showcase.Slug}?tag={category.Slug}",
                IsActive = string.Equals(categorySlug, category.Slug, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        categoryOptions.Insert(0, new CategoryTagViewModel
        {
            Label = "Tümü",
            Slug = string.Empty,
            Url = $"/{showcase.Slug}",
            IsActive = string.IsNullOrWhiteSpace(categorySlug)
        });

        var showcaseTags = showcase.TagsContent
            .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Select(tag => new CategoryTagViewModel
            {
                Label = tag,
                Slug = tag,
                Url = "#product-grid",
                IsActive = false
            })
            .ToList();

        var activeTag = categoryOptions.FirstOrDefault(x => x.IsActive);
        var sortFilter = document.Filters.FirstOrDefault(x => string.Equals(x.Group, "Sırala", StringComparison.OrdinalIgnoreCase))
            ?? document.Filters.FirstOrDefault(x => string.Equals(x.Group, "Sirala", StringComparison.OrdinalIgnoreCase));

        return new ShowcaseViewModel
        {
            Title = $"{showcase.Title} | VitaCure",
            MetaDescription = string.IsNullOrWhiteSpace(showcase.MetaDescription) ? showcase.Description : showcase.MetaDescription!,
            CanonicalPath = string.IsNullOrWhiteSpace(categorySlug) ? $"/{showcase.Slug}" : $"/{showcase.Slug}?tag={categorySlug}",
            ChatWidget = BuildChatWidget(document, categories, Array.Empty<ShowcaseSummaryViewModel>(), null, "category"),
            Showcase = new ShowcaseSummaryViewModel
            {
                Name = showcase.Name,
                Title = showcase.Title,
                Slug = showcase.Slug,
                IconClass = showcase.IconClass,
                BackgroundImageUrl = ResolveShowcaseBackgroundImage(showcase.Slug, showcase.BackgroundImageUrl),
                IsDark = showcase.IsDark
            },
            DescriptionHtml = showcase.Description,
            Tags = showcaseTags,
            FeaturedProducts = coverflowProducts,
            ProductGrid = BuildCategoryGrid(poolProducts),
            Filters = new[]
            {
                new ShowcaseFilterGroupViewModel
                {
                    Title = "Kategoriler",
                    Options = categoryOptions
                }
            },
            ResultLabel = $"{poolProducts.Count} ürün bulundu",
            SortLabel = sortFilter?.Label ?? "Sırala:",
            SortOptions = sortFilter?.SortOptions ?? Array.Empty<string>(),
            ActiveCategorySlug = activeTag?.Slug
        };
    }

    public async Task<CategoryViewModel?> GetCategoryPageContentAsync(string slug, string? tagSlug = null, CancellationToken cancellationToken = default)
    {
        var document = _document.Value;
        var categories = await BuildCategoriesAsync(cancellationToken);
        var category = categories.FirstOrDefault(x => x.Slug == slug);
        if (category is null)
        {
            return null;
        }

        var categoryProductsQuery = _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag)
            .Where(x => x.IsActive && x.Category != null && x.Category.Slug == slug);

        if (!string.IsNullOrWhiteSpace(tagSlug))
        {
            categoryProductsQuery = categoryProductsQuery
                .Where(x => x.ProductTags.Any(pt => pt.Tag != null && pt.Tag.Slug == tagSlug));
        }

        var categoryProducts = await categoryProductsQuery
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var fallbackProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name)
            .Take(12)
            .ToListAsync(cancellationToken);

        var categoryTagOptions = await BuildCategoryTagOptionsAsync(slug, tagSlug, cancellationToken);
        var sortFilter = document.Filters.FirstOrDefault(x => string.Equals(x.Group, "Sırala", StringComparison.OrdinalIgnoreCase));
        var shouldUseFallback = categoryProducts.Count == 0 && string.IsNullOrWhiteSpace(tagSlug);
        var productsForCategory = shouldUseFallback ? fallbackProducts : categoryProducts;
        var activeTag = categoryTagOptions.FirstOrDefault(x => x.IsActive);

        return new CategoryViewModel
        {
            Title = $"{category.Name} | VitaCure",
            MetaDescription = string.IsNullOrWhiteSpace(category.Description) ? BuildHomeMetaDescription(document) : category.Description,
            CanonicalPath = string.IsNullOrWhiteSpace(tagSlug) ? $"/{slug}" : $"/{slug}?tag={tagSlug}",
            Category = category,
            ChatWidget = BuildChatWidget(document, categories, Array.Empty<ShowcaseSummaryViewModel>(), category, "category"),
            HeroBanner = new BannerViewModel
            {
                Name = $"{category.Name} Banner",
                AltText = $"{category.Name} Kampanya",
                ImageUrl = category.BackgroundImageUrl,
                TargetUrl = $"/{category.Slug}"
            },
            Filters = BuildFilters(document),
            CoverflowProducts = BuildCoverflowProducts(productsForCategory),
            ProductGrid = BuildCategoryGrid(productsForCategory),
            Breadcrumbs = new[]
            {
                new BreadcrumbItemViewModel { Label = "Ana Sayfa", Url = "/", IsActive = false },
                new BreadcrumbItemViewModel { Label = category.Name, Url = $"/{category.Slug}", IsActive = true }
            },
            ResultLabel = $"{productsForCategory.Count} ürün bulundu",
            SortLabel = sortFilter?.Label ?? "Sırala:",
            SortOptions = sortFilter?.SortOptions ?? Array.Empty<string>(),
            ActiveTagSlug = activeTag?.Slug,
            ActiveTagLabel = activeTag?.Label ?? "Tümü",
            AvailableTags = categoryTagOptions
        };
    }

    public Task<IReadOnlyList<CategorySummaryViewModel>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return BuildCategoriesAsync(cancellationToken);
    }

    public async Task<ProductDetailViewModel?> GetProductDetailPageContentAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetBySlugAsync(slug, cancellationToken);
        if (product is null || product.Category is null)
        {
            return null;
        }

        var relatedProducts = await _productService.GetRelatedProductsAsync(product.CategoryId, product.Id, cancellationToken: cancellationToken);

        return new ProductDetailViewModel
        {
            Title = $"{product.Name} | VitaCure",
            MetaDescription = string.IsNullOrWhiteSpace(product.Description) ? $"{product.Name} detay sayfasi." : product.Description,
            CanonicalPath = $"/{product.Slug}",
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            GalleryImages = BuildGalleryImages(product),
            Price = FormatPrice(product.Price),
            OldPrice = product.OldPrice.HasValue ? FormatPrice(product.OldPrice.Value) : string.Empty,
            Rating = product.Rating.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
            RatingWidth = $"{Math.Round(product.Rating / 5m * 100m, MidpointRounding.AwayFromZero)}%",
            SizeLabel = BuildProductSizeLabel(product.Name),
            CategoryName = product.Category.Name,
            CategorySlug = product.Category.Slug,
            CartProductSlug = product.Slug,
            Tags = product.ProductTags
                .Where(x => x.Tag is not null)
                .Select(x => x.Tag!.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray(),
            Breadcrumbs = new[]
            {
                new BreadcrumbItemViewModel { Label = "Ana Sayfa", Url = "/", IsActive = false },
                new BreadcrumbItemViewModel { Label = product.Category.Name, Url = $"/{product.Category.Slug}", IsActive = false },
                new BreadcrumbItemViewModel { Label = product.Name, Url = $"/{product.Slug}", IsActive = true }
            },
            RelatedProducts = BuildProductCards(relatedProducts)
        };
    }

    private static IReadOnlyList<string> BuildGalleryImages(Product product)
    {
        var images = new List<string>();

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            images.Add(product.ImageUrl);
        }

        if (!string.IsNullOrWhiteSpace(product.GalleryImageUrls))
        {
            images.AddRange(product.GalleryImageUrls
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        return images
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<IReadOnlyList<CategoryTagViewModel>> BuildCategoryTagOptionsAsync(string categorySlug, string? activeTagSlug, CancellationToken cancellationToken)
    {
        var tagItems = await _dbContext.ProductTags
            .AsNoTracking()
            .Where(x => x.Product != null && x.Tag != null && x.Product.IsActive && x.Product.Category != null && x.Product.Category.Slug == categorySlug)
            .Select(x => new
            {
                x.Tag!.Name,
                x.Tag.Slug
            })
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = new List<CategoryTagViewModel>
        {
            new()
            {
                Label = "Tümü",
                Slug = string.Empty,
                Url = $"/{categorySlug}",
                IsActive = string.IsNullOrWhiteSpace(activeTagSlug)
            }
        };

        items.AddRange(tagItems.Select(tag => new CategoryTagViewModel
        {
            Label = tag.Name,
            Slug = tag.Slug,
            Url = $"/{categorySlug}?tag={tag.Slug}",
            IsActive = string.Equals(tag.Slug, activeTagSlug, StringComparison.OrdinalIgnoreCase)
        }));

        return items;
    }

    private StorefrontUiDocument LoadDocument()
    {
        var path = Path.Combine(_environment.ContentRootPath, "docs", "mock-data.json");
        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<StorefrontUiDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return document ?? new StorefrontUiDocument();
    }

    private async Task<IReadOnlyList<CategorySummaryViewModel>> BuildCategoriesAsync(CancellationToken cancellationToken)
    {
        var document = _document.Value;
        var categoryEntities = await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.IsActive && x.Slug != "uncategorized")
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var backgrounds = new Dictionary<string, (string Image, string Overlay, string Pill, string BackgroundClass)>(StringComparer.OrdinalIgnoreCase)
        {
            ["uyku-sagligi"] = ("/img/uykuBg.png", "linear-gradient(to right, rgba(0,0,0,0.85) 0%, rgba(0,0,0,0.6) 40%, rgba(0,0,0,0.1) 100%)", "bg-uyku", "category-theme-sleep"),
            ["multivitamin-enerji"] = ("/img/multivitaminBg.png", "linear-gradient(to right, rgba(88,38,0,0.92) 0%, rgba(154,84,16,0.65) 50%, rgba(0,0,0,0.1) 100%)", "bg-multi", "category-theme-energy"),
            ["zihin-hafiza-guclendirme"] = ("/img/zekaHafızaBg.png", "linear-gradient(to right, rgba(32,10,64,0.9) 0%, rgba(113,64,160,0.6) 50%, rgba(0,0,0,0.08) 100%)", "bg-zihin", "category-theme-mind"),
            ["hastaliklara-karsi-koruma"] = ("/img/hastalıkKorumaBg.png", "linear-gradient(to right, rgba(3,54,56,0.92) 0%, rgba(13,111,104,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-koruma", "category-theme-protection"),
            ["kas-ve-iskelet-sagligi"] = ("/img/kasİskeletBg.png", "linear-gradient(to right, rgba(70,9,27,0.92) 0%, rgba(182,54,95,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-kas", "category-theme-muscle"),
            ["zayiflama-destegi"] = ("/img/zayıflamaBg.png", "linear-gradient(to right, rgba(86,56,0,0.92) 0%, rgba(198,136,0,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-zayiflama", "category-theme-weight")
        };

        var descriptionHtml = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["uyku-sagligi"] = "<strong>Melatonin</strong>, <strong>magnezyum</strong>, <strong>bitkisel ekstreler</strong>, <strong>gece rutini destekleri</strong> ve <strong>çocuklara uygun uyku takviyeleri</strong> ile daha derin, kesintisiz ve dinlendirici uyku deneyimi için özenle seçilmiş ürünleri keşfedin; <strong>rahatlama</strong>, <strong>gevşeme</strong>, <strong>uykuya geçiş</strong> ve <strong>sabah zindeliği</strong>ni destekleyen güçlü formüller bu kategoride sizi bekliyor."
        };

        return categoryEntities.Select(category =>
        {
            var sourceCategory = document.Categories.FirstOrDefault(x => x.SlugCandidate == category.Slug);
            var chatMeta = document.ChatWidget.ByCategory.TryGetValue(category.Slug, out var item) ? item : new ChatCategory();
            var background = backgrounds.TryGetValue(category.Slug, out var bg)
                ? bg
                : (Image: "/img/banners/vitacureai.png", Overlay: "linear-gradient(to right, rgba(0,0,0,0.78) 0%, rgba(0,0,0,0.46) 50%, rgba(0,0,0,0.08) 100%)", Pill: "bg-uyku", BackgroundClass: "category-theme-default");

            return new CategorySummaryViewModel
            {
                Name = category.Name,
                DisplayName = chatMeta.DisplayName ?? category.Name,
                Slug = category.Slug,
                IconClass = sourceCategory?.Icon ?? string.Empty,
                PillCssClass = background.Pill,
                Description = category.Description,
                DescriptionHtml = descriptionHtml.TryGetValue(category.Slug, out var htmlDescription) ? htmlDescription : category.Description,
                BackgroundImageUrl = background.Image,
                BackgroundOverlay = background.Overlay,
                BackgroundClass = background.BackgroundClass,
                Tags = chatMeta.TagButtons is { Length: > 0 } ? chatMeta.TagButtons : new[] { "Tümü" }
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<ShowcaseSummaryViewModel>> BuildHomeShowcasesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Showcases
            .AsNoTracking()
            .Include(x => x.ShowcaseCategories)
            .Where(x => x.IsActive && x.ShowOnHome)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Take(6)
            .Select(x => new ShowcaseSummaryViewModel
            {
                Name = x.Name,
                Title = x.Title,
                Slug = x.Slug,
                CategorySlug = x.ShowcaseCategories
                    .OrderBy(sc => sc.CategoryId)
                    .Select(sc => sc.Category != null ? sc.Category.Slug : string.Empty)
                    .FirstOrDefault() ?? string.Empty,
                IconClass = x.IconClass,
                PillCssClass = ResolveShowcasePillClass(
                    x.ShowcaseCategories
                        .OrderBy(sc => sc.CategoryId)
                        .Select(sc => sc.Category != null ? sc.Category.Slug : string.Empty)
                        .FirstOrDefault(),
                    x.Slug),
                BackgroundImageUrl = ResolveShowcaseBackgroundImage(x.Slug, x.BackgroundImageUrl),
                IsDark = x.IsDark
            })
            .ToListAsync(cancellationToken);
    }

    private static string ResolveShowcaseBackgroundImage(string? slug, string? currentValue)
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            return currentValue;
        }

        var explicitMatch = slug?.Trim().ToLowerInvariant() switch
        {
            "uyku-sagligi" or "uyku-rutini" => "/img/uykuBg.png",
            "multivitamin-enerji" or "multivitamin-enerji-plani" => "/img/multivitaminBg.png",
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "/img/zekaHafızaBg.png",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "/img/hastalıkKorumaBg.png",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "/img/kasİskeletBg.png",
            "zayiflama-destegi" or "zayiflama-rotasi" => "/img/zayıflamaBg.png",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(explicitMatch)
            ? currentValue ?? string.Empty
            : explicitMatch;
    }

    private static string ResolveShowcasePillClass(string? categorySlug, string? showcaseSlug)
    {
        var normalizedSlug = !string.IsNullOrWhiteSpace(categorySlug) ? categorySlug : showcaseSlug;

        return normalizedSlug?.Trim().ToLowerInvariant() switch
        {
            "uyku-sagligi" or "uyku-rutini" => "bg-uyku",
            "multivitamin-enerji" or "multivitamin-enerji-plani" => "bg-multi",
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "bg-zihin",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "bg-koruma",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "bg-kas",
            "zayiflama-destegi" or "zayiflama-rotasi" => "bg-zayiflama",
            _ => string.Empty
        };
    }

    private static string ResolveShowcaseCategorySlug(string? showcaseSlug)
    {
        return showcaseSlug?.Trim().ToLowerInvariant() switch
        {
            "uyku-sagligi" or "uyku-rutini" => "uyku-sagligi",
            "multivitamin-enerji" or "multivitamin-enerji-plani" => "multivitamin-enerji",
            "zihin-hafiza-guclendirme" or "zihin-hafiza-rotasi" => "zihin-hafiza-guclendirme",
            "hastaliklara-karsi-koruma" or "bagisiklik-koruma-plani" => "hastaliklara-karsi-koruma",
            "kas-ve-iskelet-sagligi" or "kas-iskelet-destegi" => "kas-ve-iskelet-sagligi",
            "zayiflama-destegi" or "zayiflama-rotasi" => "zayiflama-destegi",
            _ => string.Empty
        };
    }

    private static ChatWidgetViewModel BuildChatWidget(
        StorefrontUiDocument data,
        IReadOnlyList<CategorySummaryViewModel> categories,
        IReadOnlyList<ShowcaseSummaryViewModel> showcases,
        CategorySummaryViewModel? category,
        string variant,
        HomeChatConfiguration? homeChat = null)
    {
        var global = data.ChatWidget.Global;
        var prompts = category is null
            ? Array.Empty<string>()
            : data.ExamplePrompts.ByCategory.TryGetValue(category.Slug, out var categoryPrompts)
                ? categoryPrompts
                : Array.Empty<string>();

        return new ChatWidgetViewModel
        {
            Variant = variant,
            HeaderTitle = category?.DisplayName ?? string.Empty,
            HeaderBackUrl = "/",
            HeroTitle = category is null ? homeChat?.HeroTitle ?? global.HeroTitle : global.HeroTitle,
            HeroSubtitle = category is null ? homeChat?.HeroSubtitle ?? global.HeroSubtitle : global.HeroSubtitle,
            CompactBackLabel = global.CompactBackLabel,
            CompactCategoryLabel = global.CompactCategoryLabel,
            SearchFilterLabel = global.SearchFilterLabel,
            MainPlaceholder = category is null ? homeChat?.MainPlaceholder ?? global.MainPlaceholder : global.MainPlaceholder,
            FullscreenTitle = global.FullscreenTitle,
            AddFileTitle = global.AddFileTitle,
            ChatModeLabel = global.ChatModeLabel,
            SearchModeLabel = global.SearchModeLabel,
            FileMenuDocumentLabel = global.FileMenuDocumentLabel,
            FileMenuImageLabel = global.FileMenuImageLabel,
            SearchPlaceholder = category is null ? homeChat?.SearchPlaceholder ?? global.SearchPlaceholder : global.SearchPlaceholder,
            SearchPlaceholderLocked = category is null
                ? homeChat?.SearchPlaceholderLocked ?? global.SearchPlaceholderLocked
                : "Bu kategoride aramak istediğiniz ürünü yazın...",
            CategorySlug = category?.Slug ?? string.Empty,
            CategoryName = category?.DisplayName ?? string.Empty,
            Showcases = showcases,
            Categories = categories,
            ExamplePrompts = prompts,
            PromptPoolByCategory = data.ExamplePrompts.ByCategory.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value),
            TagButtonsByCategory = data.ChatWidget.ByCategory.ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)(x.Value.TagButtons ?? Array.Empty<string>()))
        };
    }

    private static IReadOnlyList<ProductCardViewModel> BuildProductCards(IReadOnlyList<Product> items)
    {
        return items.Select(item => new ProductCardViewModel
        {
            Id = item.Slug,
            Name = item.Name,
            SizeLabel = BuildProductSizeLabel(item.Name),
            ImageUrl = item.ImageUrl,
            Price = item.Price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ","),
            OldPrice = item.OldPrice?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",") ?? string.Empty,
            Rating = item.Rating.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
            RatingWidth = $"{Math.Round(item.Rating / 5m * 100m, MidpointRounding.AwayFromZero)}%",
            Description = item.Description ?? string.Empty,
            Href = $"/{item.Slug}",
            CartProductSlug = item.Slug
        }).ToList();
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
    }

    private static IReadOnlyList<ProductCardViewModel> BuildCategoryGrid(IReadOnlyList<Product> products)
    {
        return BuildProductCards(products);
    }

    private static IReadOnlyList<ProductCardViewModel> BuildCoverflowProducts(IReadOnlyList<Product> products)
    {
        return BuildProductCards(products);
    }

    private static string BuildProductSizeLabel(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "60 kapsul";
        }

        return productName.Trim() switch
        {
            "Daily Multivitamin" => "120 kapsul",
            "Omega 3" => "60 softgel",
            "Vitamin D3" => "30 ml damla",
            "Magnezyum" => "60 kapsul",
            "C Vitamini Complex" => "20 efervesan tablet",
            "Kolajen Peptit" => "300 gr toz",
            "B12 Vitamini" => "30 ml sprey",
            "Çinko Pikolinat" => "90 kapsul",
            "Probiyotik 10B" => "30 kapsul",
            "Demir + C Vitamini" => "30 kapsul",
            "Kalsiyum Kompleks" => "60 tablet",
            "B12 Vitamini Sprey" => "20 ml sprey",
            "Vitamin D3 - 3 Al 2 Öde" => "3 x 30 ml",
            "Omega 3 Aile Paketi" => "2 x 60 softgel",
            "C Vitamini Seti" => "3 x 20 tablet",
            "Magnezyum Enerji Kofre" => "2 x 60 kapsul",
            "Daily Multivitamin Büyük Boy" => "180 kapsul",
            "Kolajen ve C Vitamini" => "14 saşe",
            "Çinko Kompleks" => "60 tablet",
            "Probiyotik Bakteri" => "20 kapsul",
            "Demir Takviyesi" => "30 kapsul",
            _ => "60 kapsul"
        };
    }

    private static IReadOnlyList<FilterGroupViewModel> BuildFilters(StorefrontUiDocument data)
    {
        return data.Filters.Select((filter, filterIndex) => new FilterGroupViewModel
        {
            Group = filter.Group,
            PanelTitle = filter.PanelTitle ?? string.Empty,
            ClearLabel = filter.ClearLabel ?? string.Empty,
            ResultLabel = filter.ResultLabel ?? string.Empty,
            Label = filter.Label ?? string.Empty,
            SortOptions = filter.SortOptions,
            Options = filter.OptionItems.Select((option, optionIndex) => new FilterOptionViewModel
            {
                Id = $"filter-{filterIndex}-{optionIndex}",
                Label = option.Label,
                Count = option.Count
            }).ToArray()
        }).ToList();
    }

    private static IReadOnlyList<BannerViewModel> BuildPopularSupplementCards(StorefrontUiDocument data)
    {
        var gradients = new[]
        {
            "linear-gradient(135deg, #ebd15e, #f5e48a)",
            "linear-gradient(135deg, #c9404d, #e35a66)",
            "linear-gradient(135deg, #7baef2, #94c4ff)",
            "linear-gradient(135deg, #8dc997, #a4e3af)",
            "linear-gradient(135deg, #a67feb, #be9df2)",
            "linear-gradient(135deg, #62c3d9, #7fd5e8)",
            "linear-gradient(135deg, #c79650, #ddb173)",
            "linear-gradient(135deg, #94a7c2, #abc0db)",
            "linear-gradient(135deg, #e38874, #f5a08e)"
        };

        return data.Campaigns
            .Where(x => x.Type == "popular-supplement-card")
            .Select((item, index) => new BannerViewModel
            {
                Name = item.Name,
                Title = item.Name,
                AltText = item.Name,
                ImageUrl = NormalizeImageUrl(item.Image),
                TargetUrl = "#",
                Gradient = gradients[index % gradients.Length],
                TextColor = "#ffffff"
            })
            .ToList();
    }

    private static string NormalizeImageUrl(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return string.Empty;
        }

        if (imagePath.StartsWith("/", StringComparison.Ordinal))
        {
            return imagePath;
        }

        return $"/img/{imagePath.TrimStart('/')}";
    }

    private static IReadOnlyList<BannerViewModel> BuildCampaignBanners(StorefrontUiDocument data)
    {
        var banner = data.Banners.FirstOrDefault(x => x.ImageFiles.Count > 0);
        if (banner is null)
        {
            return Array.Empty<BannerViewModel>();
        }

        return banner.ImageFiles.Select(file => new BannerViewModel
        {
            Name = file,
            AltText = "Reklam Banner",
            ImageUrl = $"/img/banners/{file}",
            TargetUrl = "#"
        }).ToList();
    }

    private static string BuildHomeMetaDescription(StorefrontUiDocument data)
    {
        return data.SeoCandidates.LastOrDefault()
               ?? "Vitacure vitamin ve takviye deneyimi.";
    }

    private static string FindSectionTitle(StorefrontUiDocument data, string fallback)
    {
        return data.Sections.FirstOrDefault(x => string.Equals(x.Title, fallback, StringComparison.OrdinalIgnoreCase))?.Title ?? fallback;
    }

    private class StorefrontUiDocument
    {
        public List<CategoryUiItem> Categories { get; set; } = new();
        public List<CampaignItem> Campaigns { get; set; } = new();
        public List<BannerItem> Banners { get; set; } = new();
        public ChatWidgetRoot ChatWidget { get; set; } = new();
        public ExamplePromptsRoot ExamplePrompts { get; set; } = new();
        public List<FilterItem> Filters { get; set; } = new();
        public List<SectionItem> Sections { get; set; } = new();
        public List<string> SeoCandidates { get; set; } = new();
    }

    private class CategoryUiItem
    {
        public string Name { get; set; } = string.Empty;
        public string SlugCandidate { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    private class CampaignItem
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    private class BannerItem
    {
        public List<string> ImageFiles { get; set; } = new();
    }

    private class ChatWidgetRoot
    {
        public ChatGlobal Global { get; set; } = new();
        public Dictionary<string, ChatCategory> ByCategory { get; set; } = new();
    }

    private class ChatGlobal
    {
        public string HeroTitle { get; set; } = string.Empty;
        public string HeroSubtitle { get; set; } = string.Empty;
        public string CompactBackLabel { get; set; } = string.Empty;
        public string CompactCategoryLabel { get; set; } = string.Empty;
        public string SearchFilterLabel { get; set; } = string.Empty;
        public string MainPlaceholder { get; set; } = string.Empty;
        public string FullscreenTitle { get; set; } = string.Empty;
        public string AddFileTitle { get; set; } = string.Empty;
        public string ChatModeLabel { get; set; } = string.Empty;
        public string SearchModeLabel { get; set; } = string.Empty;
        public string FileMenuDocumentLabel { get; set; } = string.Empty;
        public string FileMenuImageLabel { get; set; } = string.Empty;
        public string SearchPlaceholder { get; set; } = string.Empty;
        public string SearchPlaceholderLocked { get; set; } = string.Empty;
    }

    private class ChatCategory
    {
        public string? DisplayName { get; set; }
        public string[]? TagButtons { get; set; }
    }

    private class ExamplePromptsRoot
    {
        public Dictionary<string, string[]> ByCategory { get; set; } = new();
    }

    private class FilterItem
    {
        public string Group { get; set; } = string.Empty;
        public string? PanelTitle { get; set; }
        public string? ClearLabel { get; set; }
        public string? ResultLabel { get; set; }
        public string? Label { get; set; }
        public List<FilterOptionItem> OptionItems { get; set; } = new();
        public string[] SortOptions { get; set; } = Array.Empty<string>();

        [System.Text.Json.Serialization.JsonPropertyName("options")]
        public JsonElement Options
        {
            set
            {
                if (value.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                if (value.EnumerateArray().All(item => item.ValueKind == JsonValueKind.String))
                {
                    SortOptions = value.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray();
                    return;
                }

                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    OptionItems.Add(new FilterOptionItem
                    {
                        Label = item.GetProperty("label").GetString() ?? string.Empty,
                        Count = item.GetProperty("count").GetString() ?? string.Empty
                    });
                }
            }
        }
    }

    private class FilterOptionItem
    {
        public string Label { get; set; } = string.Empty;
        public string Count { get; set; } = string.Empty;
    }

    private class SectionItem
    {
        public string Title { get; set; } = string.Empty;
    }
}
