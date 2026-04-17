using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class HomeContentFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }

    [Required]
    [Display(Name = "Hero Başlığı")]
    public string HeroTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Hero Alt Başlığı")]
    public string HeroSubtitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ana Placeholder")]
    public string MainPlaceholder { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Arama Placeholder")]
    public string SearchPlaceholder { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kilitli Arama Placeholder")]
    public string SearchPlaceholderLocked { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Öne Çıkan Başlık")]
    public string FeaturedTitle { get; set; } = string.Empty;

    [Display(Name = "Öne Çıkan CTA Metni")]
    public string? FeaturedActionLabel { get; set; }

    [Display(Name = "Öne Çıkan CTA Linki")]
    public string? FeaturedActionUrl { get; set; }

    [Required]
    [Display(Name = "Popüler Takviyeler Başlığı")]
    public string PopularTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kampanyalar Başlığı")]
    public string CampaignsTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fırsat Ürünleri Başlığı")]
    public string DealsTitle { get; set; } = string.Empty;

    [Display(Name = "Fırsat CTA Metni")]
    public string? DealsActionLabel { get; set; }

    [Display(Name = "Fırsat CTA Linki")]
    public string? DealsActionUrl { get; set; }

    [Required]
    [Display(Name = "Banner Adı")]
    public string FeaturedBannerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner Alt Metni")]
    public string FeaturedBannerAltText { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner Görsel URL")]
    public string FeaturedBannerImageUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner Hedef Linki")]
    public string FeaturedBannerTargetUrl { get; set; } = string.Empty;

    [Display(Name = "Popüler Takviyeler")]
    public string PopularSupplementsContent { get; set; } = string.Empty;

    [Display(Name = "Kampanya Bannerları")]
    public string CampaignBannersContent { get; set; } = string.Empty;
}
