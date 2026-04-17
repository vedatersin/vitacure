using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public class HomeContentConfigurationService : IHomeContentConfigurationService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly Lazy<HomeContentDocument> _document;

    public HomeContentConfigurationService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
        _document = new Lazy<HomeContentDocument>(LoadDocument, true);
    }

    public Task<HomeContentConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
        => GetConfigurationCoreAsync(useFallbackOnly: false, cancellationToken);

    public Task<HomeContentConfiguration> GetFallbackConfigurationAsync(CancellationToken cancellationToken = default)
        => GetConfigurationCoreAsync(useFallbackOnly: true, cancellationToken);

    private async Task<HomeContentConfiguration> GetConfigurationCoreAsync(bool useFallbackOnly, CancellationToken cancellationToken)
    {
        if (!useFallbackOnly)
        {
            var entity = await _dbContext.HomeContentSettings
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity is not null)
            {
                return MapFromEntity(entity);
            }
        }

        var document = _document.Value;

        return new HomeContentConfiguration
        {
            MetaDescription = document.SeoCandidates.LastOrDefault() ?? "Vitacure vitamin ve takviye deneyimi.",
            Chat = new HomeChatConfiguration
            {
                HeroTitle = document.ChatWidget.Global.HeroTitle,
                HeroSubtitle = document.ChatWidget.Global.HeroSubtitle,
                MainPlaceholder = document.ChatWidget.Global.MainPlaceholder,
                SearchPlaceholder = document.ChatWidget.Global.SearchPlaceholder,
                SearchPlaceholderLocked = document.ChatWidget.Global.SearchPlaceholderLocked
            },
            FeaturedBanner = new BannerViewModel
            {
                Name = "Vitacure AI Banner",
                AltText = "Vitacure AI Banner",
                ImageUrl = "/img/banners/vitacureai.png",
                TargetUrl = "#vitacure-ai"
            },
            PopularSupplements = BuildPopularSupplementCards(document),
            CampaignBanners = BuildCampaignBanners(document),
            SectionHeaders = BuildSectionHeaders(document)
        };
    }

    private static HomeContentConfiguration MapFromEntity(HomeContentSettings entity)
    {
        return new HomeContentConfiguration
        {
            MetaDescription = entity.MetaDescription ?? "Vitacure vitamin ve takviye deneyimi.",
            Chat = new HomeChatConfiguration
            {
                HeroTitle = entity.HeroTitle,
                HeroSubtitle = entity.HeroSubtitle,
                MainPlaceholder = entity.MainPlaceholder,
                SearchPlaceholder = entity.SearchPlaceholder,
                SearchPlaceholderLocked = entity.SearchPlaceholderLocked
            },
            FeaturedBanner = new BannerViewModel
            {
                Name = entity.FeaturedBannerName,
                AltText = entity.FeaturedBannerAltText,
                ImageUrl = entity.FeaturedBannerImageUrl,
                TargetUrl = entity.FeaturedBannerTargetUrl
            },
            PopularSupplements = ParsePopularSupplements(entity.PopularSupplementsContent),
            CampaignBanners = ParseCampaignBanners(entity.CampaignBannersContent),
            SectionHeaders = new Dictionary<string, SectionHeaderViewModel>(StringComparer.OrdinalIgnoreCase)
            {
                ["featured"] = new()
                {
                    Title = entity.FeaturedTitle,
                    ActionLabel = entity.FeaturedActionLabel,
                    ActionUrl = entity.FeaturedActionUrl
                },
                ["popular"] = new()
                {
                    Title = entity.PopularTitle
                },
                ["campaigns"] = new()
                {
                    Title = entity.CampaignsTitle
                },
                ["deals"] = new()
                {
                    Title = entity.DealsTitle,
                    ActionLabel = entity.DealsActionLabel,
                    ActionUrl = entity.DealsActionUrl
                }
            }
        };
    }

    private static IDictionary<string, SectionHeaderViewModel> BuildSectionHeaders(HomeContentDocument document)
    {
        return new Dictionary<string, SectionHeaderViewModel>(StringComparer.OrdinalIgnoreCase)
        {
            ["featured"] = new()
            {
                Title = FindSectionTitle(document, "Öne Çıkan Ürünler"),
                ActionLabel = "Tümünü Gör",
                ActionUrl = "/#tum-urunler"
            },
            ["popular"] = new()
            {
                Title = FindSectionTitle(document, "Popüler Takviyeler")
            },
            ["campaigns"] = new()
            {
                Title = FindSectionTitle(document, "Kampanyalar")
            },
            ["deals"] = new()
            {
                Title = FindSectionTitle(document, "Fırsat Ürünleri"),
                ActionLabel = "Tümünü Gör",
                ActionUrl = "/#firsat-urunleri"
            }
        };
    }

    private static IReadOnlyList<BannerViewModel> BuildPopularSupplementCards(HomeContentDocument document)
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

        return document.Campaigns
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

    private static IReadOnlyList<BannerViewModel> BuildCampaignBanners(HomeContentDocument document)
    {
        var banner = document.Banners.FirstOrDefault(x => x.ImageFiles.Count > 0);
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

    private static IReadOnlyList<BannerViewModel> ParsePopularSupplements(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<BannerViewModel>();
        }

        return content
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line =>
            {
                var parts = line.Split('|', StringSplitOptions.TrimEntries);
                var title = parts.ElementAtOrDefault(0) ?? string.Empty;
                return new BannerViewModel
                {
                    Name = title,
                    Title = title,
                    AltText = title,
                    ImageUrl = parts.ElementAtOrDefault(1) ?? string.Empty,
                    TargetUrl = parts.ElementAtOrDefault(2) ?? "#",
                    Gradient = parts.ElementAtOrDefault(3) ?? "linear-gradient(135deg, #7baef2, #94c4ff)",
                    TextColor = parts.ElementAtOrDefault(4) ?? "#ffffff"
                };
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.ImageUrl))
            .ToList();
    }


    private static IReadOnlyList<BannerViewModel> ParseCampaignBanners(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<BannerViewModel>();
        }

        return content
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line =>
            {
                var parts = line.Split('|', StringSplitOptions.TrimEntries);
                return new BannerViewModel
                {
                    Name = parts.ElementAtOrDefault(2) ?? parts.ElementAtOrDefault(0) ?? string.Empty,
                    ImageUrl = parts.ElementAtOrDefault(0) ?? string.Empty,
                    TargetUrl = parts.ElementAtOrDefault(1) ?? "#",
                    AltText = parts.ElementAtOrDefault(2) ?? "Reklam Banner"
                };
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.ImageUrl))
            .ToList();
    }

    private HomeContentDocument LoadDocument()
    {
        var path = Path.Combine(_environment.ContentRootPath, "docs", "mock-data.json");
        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<HomeContentDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return document ?? new HomeContentDocument();
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

    private static string FindSectionTitle(HomeContentDocument document, string fallback)
    {
        return document.Sections.FirstOrDefault(x => string.Equals(x.Title, fallback, StringComparison.OrdinalIgnoreCase))?.Title ?? fallback;
    }

    private sealed class HomeContentDocument
    {
        public List<CampaignItem> Campaigns { get; set; } = new();
        public List<BannerItem> Banners { get; set; } = new();
        public ChatWidgetRoot ChatWidget { get; set; } = new();
        public List<SectionItem> Sections { get; set; } = new();
        public List<string> SeoCandidates { get; set; } = new();
    }

    private sealed class CampaignItem
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    private sealed class BannerItem
    {
        public List<string> ImageFiles { get; set; } = new();
    }

    private sealed class ChatWidgetRoot
    {
        public ChatGlobal Global { get; set; } = new();
    }

    private sealed class ChatGlobal
    {
        public string HeroTitle { get; set; } = string.Empty;
        public string HeroSubtitle { get; set; } = string.Empty;
        public string MainPlaceholder { get; set; } = string.Empty;
        public string SearchPlaceholder { get; set; } = string.Empty;
        public string SearchPlaceholderLocked { get; set; } = string.Empty;
    }

    private sealed class SectionItem
    {
        public string Title { get; set; } = string.Empty;
    }
}
