using vitacure.Models.ViewModels;

namespace vitacure.Services.Content;

public interface IHomeContentConfigurationService
{
    Task<HomeContentConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<HomeContentConfiguration> GetFallbackConfigurationAsync(CancellationToken cancellationToken = default);
}

public class HomeContentConfiguration
{
    public string MetaDescription { get; set; } = string.Empty;
    public HomeChatConfiguration Chat { get; set; } = new();
    public BannerViewModel FeaturedBanner { get; set; } = new();
    public IReadOnlyList<BannerViewModel> PopularSupplements { get; set; } = Array.Empty<BannerViewModel>();
    public IReadOnlyList<BannerViewModel> CampaignBanners { get; set; } = Array.Empty<BannerViewModel>();
    public IDictionary<string, SectionHeaderViewModel> SectionHeaders { get; set; } = new Dictionary<string, SectionHeaderViewModel>();
}

public class HomeChatConfiguration
{
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
    public string MainPlaceholder { get; set; } = string.Empty;
    public string SearchPlaceholder { get; set; } = string.Empty;
    public string SearchPlaceholderLocked { get; set; } = string.Empty;
}
