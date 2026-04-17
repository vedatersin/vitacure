namespace vitacure.Domain.Entities;

public class HomeContentSettings
{
    public int Id { get; set; }
    public string? MetaDescription { get; set; }

    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
    public string MainPlaceholder { get; set; } = string.Empty;
    public string SearchPlaceholder { get; set; } = string.Empty;
    public string SearchPlaceholderLocked { get; set; } = string.Empty;

    public string FeaturedTitle { get; set; } = string.Empty;
    public string? FeaturedActionLabel { get; set; }
    public string? FeaturedActionUrl { get; set; }

    public string PopularTitle { get; set; } = string.Empty;
    public string CampaignsTitle { get; set; } = string.Empty;
    public string DealsTitle { get; set; } = string.Empty;
    public string? DealsActionLabel { get; set; }
    public string? DealsActionUrl { get; set; }

    public string FeaturedBannerName { get; set; } = string.Empty;
    public string FeaturedBannerAltText { get; set; } = string.Empty;
    public string FeaturedBannerImageUrl { get; set; } = string.Empty;
    public string FeaturedBannerTargetUrl { get; set; } = string.Empty;
    public string PopularSupplementsContent { get; set; } = string.Empty;
    public string CampaignBannersContent { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
