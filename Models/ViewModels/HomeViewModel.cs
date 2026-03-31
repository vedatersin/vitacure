namespace vitacure.Models.ViewModels;

public class HomeViewModel
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalPath { get; set; } = "/";
    public ChatWidgetViewModel ChatWidget { get; set; } = new();
    public IReadOnlyList<CategorySummaryViewModel> Categories { get; set; } = Array.Empty<CategorySummaryViewModel>();
    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<BannerViewModel> PopularSupplements { get; set; } = Array.Empty<BannerViewModel>();
    public BannerViewModel? FeaturedBanner { get; set; }
    public IReadOnlyList<BannerViewModel> CampaignBanners { get; set; } = Array.Empty<BannerViewModel>();
    public IReadOnlyList<ProductCardViewModel> OpportunityProducts { get; set; } = Array.Empty<ProductCardViewModel>();
    public IDictionary<string, string> SectionTitles { get; set; } = new Dictionary<string, string>();
}
