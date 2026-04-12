using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public class MockContentService : IMockContentService
{
    private readonly IWebHostEnvironment _environment;
    private readonly Lazy<MockDataDocument> _document;

    public MockContentService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _document = new Lazy<MockDataDocument>(LoadDocument, true);
    }

    public HomeViewModel GetHomePageContent()
    {
        var data = _document.Value;
        var categories = BuildCategories(data);

        return new HomeViewModel
        {
            Title = "Ana Sayfa",
            MetaDescription = BuildHomeMetaDescription(data),
            CanonicalPath = "/",
            Categories = categories,
            ChatWidget = BuildChatWidget(data, categories, null, "home"),
            FeaturedProducts = BuildProductCards(data.Products.Where(x => x.UsageSections.Contains("Öne Çıkan Ürünler")).ToList()),
            PopularSupplements = BuildPopularSupplementCards(data),
            FeaturedBanner = new BannerViewModel
            {
                Name = "Vitacure AI Banner",
                AltText = "Vitacure AI Banner",
                ImageUrl = "/img/banners/vitacureai.png",
                TargetUrl = "#vitacure-ai"
            },
            CampaignBanners = BuildCampaignBanners(data),
            OpportunityProducts = BuildProductCards(data.Products.Where(x => x.UsageSections.Contains("Fırsat Ürünleri")).ToList()),
            SectionTitles = new Dictionary<string, string>
            {
                ["featured"] = FindSectionTitle(data, "Öne Çıkan Ürünler"),
                ["popular"] = FindSectionTitle(data, "Popüler Takviyeler"),
                ["campaigns"] = FindSectionTitle(data, "Kampanyalar"),
                ["deals"] = FindSectionTitle(data, "Fırsat Ürünleri")
            }
        };
    }

    public CategoryViewModel? GetCategoryPageContent(string slug)
    {
        var data = _document.Value;
        var categories = BuildCategories(data);
        var category = categories.FirstOrDefault(x => x.Slug == slug);
        if (category is null)
        {
            return null;
        }

        var sortFilter = data.Filters.FirstOrDefault(x => string.Equals(x.Group, "Sırala", StringComparison.OrdinalIgnoreCase));

        return new CategoryViewModel
        {
            Title = $"{category.Name} | VitaCure",
            MetaDescription = string.IsNullOrWhiteSpace(category.Description) ? BuildHomeMetaDescription(data) : category.Description,
            CanonicalPath = $"/{slug}",
            Category = category,
            ChatWidget = BuildChatWidget(data, categories, category, "category"),
            HeroBanner = new BannerViewModel
            {
                Name = $"{category.Name} Banner",
                AltText = $"{category.Name} Kampanya",
                ImageUrl = category.BackgroundImageUrl,
                TargetUrl = $"/{category.Slug}"
            },
            Filters = BuildFilters(data),
            CoverflowProducts = BuildProductCards(data.Products.Where(x => x.UsageSections.Contains("Uyku Coverflow")).ToList()),
            ProductGrid = BuildCategoryGrid(data, slug),
            Breadcrumbs = new[]
            {
                new BreadcrumbItemViewModel { Label = "Ana Sayfa", Url = "/", IsActive = false },
                new BreadcrumbItemViewModel { Label = category.Name, Url = $"/{category.Slug}", IsActive = true }
            },
            ResultLabel = sortFilter?.ResultLabel ?? "12 ürün bulundu",
            SortLabel = sortFilter?.Label ?? "Sırala:",
            SortOptions = sortFilter?.SortOptions ?? Array.Empty<string>()
        };
    }

    public IReadOnlyList<CategorySummaryViewModel> GetCategories() => BuildCategories(_document.Value);

    private MockDataDocument LoadDocument()
    {
        var path = Path.Combine(_environment.ContentRootPath, "docs", "mock-data.json");
        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<MockDataDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return document ?? new MockDataDocument();
    }

    private static string BuildHomeMetaDescription(MockDataDocument data)
    {
        return data.SeoCandidates.LastOrDefault()
               ?? "Vitacure vitamin ve takviye mock içerik deneyimi.";
    }

    private static string FindSectionTitle(MockDataDocument data, string fallback)
    {
        return data.Sections.FirstOrDefault(x => string.Equals(x.Title, fallback, StringComparison.OrdinalIgnoreCase))?.Title ?? fallback;
    }

    private static IReadOnlyList<CategorySummaryViewModel> BuildCategories(MockDataDocument data)
    {
        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["uyku-sagligi"] = data.Categories.FirstOrDefault(x => x.SlugCandidate == "uyku-sagligi")?.Description ?? string.Empty,
            ["multivitamin-enerji"] = "Gün içi performans, bağışıklık ve enerji dengesini destekleyen multivitamin, B grubu vitaminleri ve günlük destek formüllerini keşfedin.",
            ["zihin-hafiza-guclendirme"] = "Odaklanma, zihinsel berraklık ve hafıza desteği için seçilmiş nootropik, omega ve vitamin odaklı takviye seçeneklerine göz atın.",
            ["hastaliklara-karsi-koruma"] = "Bağışıklık, savunma sistemi ve mevsimsel korunma hedeflerine eşlik eden güçlü içerikleri tek sayfada toplayın.",
            ["kas-ve-iskelet-sagligi"] = "Kemik, eklem ve kas yapısını destekleyen magnezyum, kalsiyum, kolajen ve hareket odaklı ürünleri değerlendirin.",
            ["zayiflama-destegi"] = "Kilo yönetimi, metabolizma desteği ve iştah kontrolü hedefleri için hazırlanmış destekleyici ürünleri inceleyin."
        };

        var backgrounds = new Dictionary<string, (string Image, string Overlay, string Pill, string BackgroundClass)>(StringComparer.OrdinalIgnoreCase)
        {
            ["uyku-sagligi"] = ("/img/uykuBg.png", "linear-gradient(to right, rgba(0,0,0,0.85) 0%, rgba(0,0,0,0.6) 40%, rgba(0,0,0,0.1) 100%)", "bg-uyku", "category-theme-sleep"),
            ["multivitamin-enerji"] = ("/img/banners/pharmaton-mobil-banner.jpg", "linear-gradient(to right, rgba(88,38,0,0.92) 0%, rgba(154,84,16,0.65) 50%, rgba(0,0,0,0.1) 100%)", "bg-multi", "category-theme-energy"),
            ["zihin-hafiza-guclendirme"] = ("/img/banners/nutraxin-banner-mobile.jpg", "linear-gradient(to right, rgba(32,10,64,0.9) 0%, rgba(113,64,160,0.6) 50%, rgba(0,0,0,0.08) 100%)", "bg-zihin", "category-theme-mind"),
            ["hastaliklara-karsi-koruma"] = ("/img/banners/easyfishoil-banner-mobile.jpg", "linear-gradient(to right, rgba(3,54,56,0.92) 0%, rgba(13,111,104,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-koruma", "category-theme-protection"),
            ["kas-ve-iskelet-sagligi"] = ("/img/banners/corega-banner-mobile.jpg", "linear-gradient(to right, rgba(70,9,27,0.92) 0%, rgba(182,54,95,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-kas", "category-theme-muscle"),
            ["zayiflama-destegi"] = ("/img/banners/dynavit-mobil-banner.jpg", "linear-gradient(to right, rgba(86,56,0,0.92) 0%, rgba(198,136,0,0.62) 50%, rgba(0,0,0,0.08) 100%)", "bg-zayiflama", "category-theme-weight")
        };

        var descriptionHtml = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["uyku-sagligi"] = "<strong>Melatonin</strong>, <strong>magnezyum</strong>, <strong>bitkisel ekstreler</strong>, <strong>gece rutini destekleri</strong> ve <strong>çocuklara uygun uyku takviyeleri</strong> ile daha derin, kesintisiz ve dinlendirici uyku deneyimi için özenle seçilmiş ürünleri keşfedin; <strong>rahatlama</strong>, <strong>gevşeme</strong>, <strong>uykuya geçiş</strong> ve <strong>sabah zindeliği</strong>ni destekleyen güçlü formüller bu kategoride sizi bekliyor."
        };

        return data.Categories.Select(category =>
        {
            var chatMeta = data.ChatWidget.ByCategory.TryGetValue(category.SlugCandidate, out var item) ? item : new ChatCategory();
            var background = backgrounds.TryGetValue(category.SlugCandidate, out var bg)
                ? bg
                : (Image: "/img/banners/vitacureai.png", Overlay: "linear-gradient(to right, rgba(0,0,0,0.78) 0%, rgba(0,0,0,0.46) 50%, rgba(0,0,0,0.08) 100%)", Pill: "bg-uyku", BackgroundClass: "category-theme-default");

            return new CategorySummaryViewModel
            {
                Name = category.Name,
                DisplayName = chatMeta.DisplayName ?? category.Name,
                Slug = category.SlugCandidate,
                IconClass = category.Icon,
                PillCssClass = background.Pill,
                Description = descriptions.TryGetValue(category.SlugCandidate, out var description) ? description : string.Empty,
                DescriptionHtml = descriptionHtml.TryGetValue(category.SlugCandidate, out var htmlDescription)
                    ? htmlDescription
                    : (descriptions.TryGetValue(category.SlugCandidate, out var plainDescription) ? plainDescription : string.Empty),
                BackgroundImageUrl = background.Image,
                BackgroundOverlay = background.Overlay,
                BackgroundClass = background.BackgroundClass,
                Tags = chatMeta.TagButtons is { Length: > 0 } ? chatMeta.TagButtons : new[] { "Tümü" }
            };
        }).ToList();
    }

    private static ChatWidgetViewModel BuildChatWidget(
        MockDataDocument data,
        IReadOnlyList<CategorySummaryViewModel> categories,
        CategorySummaryViewModel? category,
        string variant)
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
            HeroTitle = global.HeroTitle,
            HeroSubtitle = global.HeroSubtitle,
            CompactBackLabel = global.CompactBackLabel,
            CompactCategoryLabel = global.CompactCategoryLabel,
            SearchFilterLabel = global.SearchFilterLabel,
            MainPlaceholder = global.MainPlaceholder,
            FullscreenTitle = global.FullscreenTitle,
            AddFileTitle = global.AddFileTitle,
            ChatModeLabel = global.ChatModeLabel,
            SearchModeLabel = global.SearchModeLabel,
            FileMenuDocumentLabel = global.FileMenuDocumentLabel,
            FileMenuImageLabel = global.FileMenuImageLabel,
            SearchPlaceholder = global.SearchPlaceholder,
            SearchPlaceholderLocked = category is null
                ? global.SearchPlaceholderLocked
                : "Bu kategoride aramak istediğiniz ürünü yazın...",
            CategorySlug = category?.Slug ?? string.Empty,
            CategoryName = category?.DisplayName ?? string.Empty,
            Categories = categories,
            ExamplePrompts = prompts,
            PromptPoolByCategory = data.ExamplePrompts.ByCategory.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value),
            TagButtonsByCategory = data.ChatWidget.ByCategory.ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)(x.Value.TagButtons ?? Array.Empty<string>()))
        };
    }

    private static IReadOnlyList<ProductCardViewModel> BuildProductCards(IReadOnlyList<ProductItem> items)
    {
        return items.Select((item, index) => new ProductCardViewModel
        {
            Id = string.IsNullOrWhiteSpace(item.Name) ? $"product-{index}" : item.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", string.Empty),
            Name = item.Name,
            SizeLabel = BuildProductSizeLabel(item.Name),
            ImageUrl = item.Image,
            Price = item.Price,
            OldPrice = item.OldPrice,
            Rating = item.Rating,
            RatingWidth = $"{Math.Round(ParseRating(item.Rating) / 5m * 100m, MidpointRounding.AwayFromZero)}%",
            Description = item.Description ?? string.Empty
        }).ToList();
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

    private static decimal ParseRating(string? value)
    {
        return decimal.TryParse(value?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    private static IReadOnlyList<ProductCardViewModel> BuildCategoryGrid(MockDataDocument data, string slug)
    {
        var relatedProducts = data.Products
            .Where(x => x.CategoryRelation.Contains(slug))
            .ToList();

        if (relatedProducts.Count == 0)
        {
            relatedProducts = data.Products.Where(x => x.UsageSections.Contains("Öne Çıkan Ürünler")).ToList();
        }

        var cards = BuildProductCards(relatedProducts);
        var grid = new List<ProductCardViewModel>();
        while (grid.Count < 12 && cards.Count > 0)
        {
            foreach (var card in cards)
            {
                if (grid.Count == 12)
                {
                    break;
                }

                grid.Add(card);
            }
        }

        return grid;
    }

    private static IReadOnlyList<FilterGroupViewModel> BuildFilters(MockDataDocument data)
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

    private static IReadOnlyList<BannerViewModel> BuildPopularSupplementCards(MockDataDocument data)
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

    private static IReadOnlyList<BannerViewModel> BuildCampaignBanners(MockDataDocument data)
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

    private class MockDataDocument
    {
        public List<CategoryItem> Categories { get; set; } = new();
        public List<ProductItem> Products { get; set; } = new();
        public List<CampaignItem> Campaigns { get; set; } = new();
        public List<BannerItem> Banners { get; set; } = new();
        public ChatWidgetRoot ChatWidget { get; set; } = new();
        public ExamplePromptsRoot ExamplePrompts { get; set; } = new();
        public List<FilterItem> Filters { get; set; } = new();
        public List<SectionItem> Sections { get; set; } = new();
        public List<string> SeoCandidates { get; set; } = new();
    }

    private class CategoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string SlugCandidate { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private class ProductItem
    {
        public string Name { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string OldPrice { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public List<string> UsageSections { get; set; } = new();
        public List<string> CategoryRelation { get; set; } = new();
        public string? Description { get; set; }
    }

    private class CampaignItem
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    private class BannerItem
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
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
        public List<string> Global { get; set; } = new();
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
