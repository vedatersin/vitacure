using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels;
using vitacure.Models.ViewModels.Admin;
using vitacure.Services.Content;

namespace vitacure.Infrastructure.Services;

public class AdminHomeContentService : IAdminHomeContentService
{
    private readonly IHomeContentConfigurationService _homeContentConfigurationService;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly AppDbContext _dbContext;

    public AdminHomeContentService(
        AppDbContext dbContext,
        ICacheInvalidationService cacheInvalidationService,
        IHomeContentConfigurationService homeContentConfigurationService)
    {
        _dbContext = dbContext;
        _cacheInvalidationService = cacheInvalidationService;
        _homeContentConfigurationService = homeContentConfigurationService;
    }

    public async Task<HomeContentFormViewModel> GetModelAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.HomeContentSettings
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is not null)
        {
            return MapToForm(entity);
        }

        var fallback = await _homeContentConfigurationService.GetFallbackConfigurationAsync(cancellationToken);
        fallback.SectionHeaders.TryGetValue("featured", out var featured);
        fallback.SectionHeaders.TryGetValue("popular", out var popular);
        fallback.SectionHeaders.TryGetValue("campaigns", out var campaigns);
        fallback.SectionHeaders.TryGetValue("deals", out var deals);

        return new HomeContentFormViewModel
        {
            MetaDescription = fallback.MetaDescription,
            HeroTitle = fallback.Chat.HeroTitle,
            HeroSubtitle = fallback.Chat.HeroSubtitle,
            MainPlaceholder = fallback.Chat.MainPlaceholder,
            SearchPlaceholder = fallback.Chat.SearchPlaceholder,
            SearchPlaceholderLocked = fallback.Chat.SearchPlaceholderLocked,
            FeaturedTitle = featured?.Title ?? string.Empty,
            FeaturedActionLabel = featured?.ActionLabel,
            FeaturedActionUrl = featured?.ActionUrl,
            PopularTitle = popular?.Title ?? string.Empty,
            CampaignsTitle = campaigns?.Title ?? string.Empty,
            DealsTitle = deals?.Title ?? string.Empty,
            DealsActionLabel = deals?.ActionLabel,
            DealsActionUrl = deals?.ActionUrl,
            FeaturedBannerName = fallback.FeaturedBanner.Name,
            FeaturedBannerAltText = fallback.FeaturedBanner.AltText,
            FeaturedBannerImageUrl = fallback.FeaturedBanner.ImageUrl,
            FeaturedBannerTargetUrl = fallback.FeaturedBanner.TargetUrl,
            PopularSupplementsContent = SerializePopularSupplements(fallback.PopularSupplements),
            CampaignBannersContent = SerializeCampaignBanners(fallback.CampaignBanners)
        };
    }

    public async Task UpdateAsync(HomeContentFormViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.HomeContentSettings
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        entity ??= new HomeContentSettings();

        entity.MetaDescription = Clean(model.MetaDescription);
        entity.HeroTitle = Required(model.HeroTitle);
        entity.HeroSubtitle = Required(model.HeroSubtitle);
        entity.MainPlaceholder = Required(model.MainPlaceholder);
        entity.SearchPlaceholder = Required(model.SearchPlaceholder);
        entity.SearchPlaceholderLocked = Required(model.SearchPlaceholderLocked);
        entity.FeaturedTitle = Required(model.FeaturedTitle);
        entity.FeaturedActionLabel = Clean(model.FeaturedActionLabel);
        entity.FeaturedActionUrl = Clean(model.FeaturedActionUrl);
        entity.PopularTitle = Required(model.PopularTitle);
        entity.CampaignsTitle = Required(model.CampaignsTitle);
        entity.DealsTitle = Required(model.DealsTitle);
        entity.DealsActionLabel = Clean(model.DealsActionLabel);
        entity.DealsActionUrl = Clean(model.DealsActionUrl);
        entity.FeaturedBannerName = Required(model.FeaturedBannerName);
        entity.FeaturedBannerAltText = Required(model.FeaturedBannerAltText);
        entity.FeaturedBannerImageUrl = Required(model.FeaturedBannerImageUrl);
        entity.FeaturedBannerTargetUrl = Required(model.FeaturedBannerTargetUrl);
        entity.PopularSupplementsContent = NormalizeMultiline(model.PopularSupplementsContent);
        entity.CampaignBannersContent = NormalizeMultiline(model.CampaignBannersContent);
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Id == 0)
        {
            _dbContext.HomeContentSettings.Add(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateStorefrontAsync(cancellationToken);
    }

    private static HomeContentFormViewModel MapToForm(HomeContentSettings entity)
    {
        return new HomeContentFormViewModel
        {
            Id = entity.Id,
            MetaDescription = entity.MetaDescription,
            HeroTitle = entity.HeroTitle,
            HeroSubtitle = entity.HeroSubtitle,
            MainPlaceholder = entity.MainPlaceholder,
            SearchPlaceholder = entity.SearchPlaceholder,
            SearchPlaceholderLocked = entity.SearchPlaceholderLocked,
            FeaturedTitle = entity.FeaturedTitle,
            FeaturedActionLabel = entity.FeaturedActionLabel,
            FeaturedActionUrl = entity.FeaturedActionUrl,
            PopularTitle = entity.PopularTitle,
            CampaignsTitle = entity.CampaignsTitle,
            DealsTitle = entity.DealsTitle,
            DealsActionLabel = entity.DealsActionLabel,
            DealsActionUrl = entity.DealsActionUrl,
            FeaturedBannerName = entity.FeaturedBannerName,
            FeaturedBannerAltText = entity.FeaturedBannerAltText,
            FeaturedBannerImageUrl = entity.FeaturedBannerImageUrl,
            FeaturedBannerTargetUrl = entity.FeaturedBannerTargetUrl,
            PopularSupplementsContent = entity.PopularSupplementsContent,
            CampaignBannersContent = entity.CampaignBannersContent
        };
    }

    private static string SerializePopularSupplements(IReadOnlyList<BannerViewModel> items)
    {
        return string.Join(Environment.NewLine, items.Select(item =>
            string.Join(" | ", new[]
            {
                item.Title ?? item.Name,
                item.ImageUrl,
                item.TargetUrl,
                item.Gradient,
                item.TextColor
            }.Where(value => !string.IsNullOrWhiteSpace(value)))));
    }

    private static string SerializeCampaignBanners(IReadOnlyList<BannerViewModel> items)
    {
        return string.Join(Environment.NewLine, items.Select(item =>
            string.Join(" | ", new[]
            {
                item.ImageUrl,
                item.TargetUrl,
                item.AltText
            }.Where(value => !string.IsNullOrWhiteSpace(value)))));
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Required(string value)
        => value.Trim();

    private static string NormalizeMultiline(string value)
        => string.Join(Environment.NewLine, value
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
