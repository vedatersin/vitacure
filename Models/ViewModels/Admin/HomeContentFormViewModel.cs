using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class HomeContentFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Meta A�iklama")]
    public string? MetaDescription { get; set; }

    [Required]
    [Display(Name = "Hero Basligi")]
    public string HeroTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Hero Alt Basligi")]
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
    [Display(Name = "�ne �ikan Baslik")]
    public string FeaturedTitle { get; set; } = string.Empty;

    [Display(Name = "�ne �ikan CTA Metni")]
    public string? FeaturedActionLabel { get; set; }

    [Display(Name = "�ne �ikan CTA Linki")]
    public string? FeaturedActionUrl { get; set; }

    [Required]
    [Display(Name = "Pop�ler Takviyeler Basligi")]
    public string PopularTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kampanyalar Basligi")]
    public string CampaignsTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Firsat �r�nleri Basligi")]
    public string DealsTitle { get; set; } = string.Empty;

    [Display(Name = "Firsat CTA Metni")]
    public string? DealsActionLabel { get; set; }

    [Display(Name = "Firsat CTA Linki")]
    public string? DealsActionUrl { get; set; }

    [Required]
    [Display(Name = "Banner Adi")]
    public string FeaturedBannerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner Alt Metni")]
    public string FeaturedBannerAltText { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner G�rsel URL")]
    public string FeaturedBannerImageUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banner Hedef Linki")]
    public string FeaturedBannerTargetUrl { get; set; } = string.Empty;

    [Display(Name = "Pop�ler Takviyeler")]
    public string PopularSupplementsContent { get; set; } = string.Empty;

    [Display(Name = "Kampanya Bannerlari")]
    public string CampaignBannersContent { get; set; } = string.Empty;
}
