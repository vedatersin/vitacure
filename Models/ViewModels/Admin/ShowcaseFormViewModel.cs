using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace vitacure.Models.ViewModels.Admin;

public class ShowcaseFormViewModel : IValidatableObject
{
    public const int MaxDescriptionLength = 630;

    public int? Id { get; set; }

    [Required]
    [Display(Name = "Vitrin Adi")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ikon Sinifi")]
    public string IconClass { get; set; } = "fa-solid fa-sparkles";

    [Required]
    [Display(Name = "Vitrin Basligi")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Aciklama")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Etiketler")]
    public string TagsContent { get; set; } = string.Empty;

    [Display(Name = "Arkaplan Gorseli")]
    public string BackgroundImageUrl { get; set; } = string.Empty;

    [Display(Name = "Arkaplan Dosyasi")]
    public IFormFile? BackgroundImageFile { get; set; }

    [Display(Name = "Vitrin Modu")]
    public bool IsDark { get; set; } = true;

    [Display(Name = "SEO Basligi")]
    public string? SeoTitle { get; set; }

    [Display(Name = "Meta Aciklama")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Ana Sayfada Goster")]
    public bool ShowOnHome { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Sira")]
    public int SortOrder { get; set; }

    public IReadOnlyList<ShowcaseBackgroundOptionViewModel> BackgroundOptions { get; set; } = Array.Empty<ShowcaseBackgroundOptionViewModel>();

    public IReadOnlyList<ShowcaseCategoryOptionViewModel> CategoryOptions { get; set; } = Array.Empty<ShowcaseCategoryOptionViewModel>();
    public List<int> SelectedCategoryIds { get; set; } = new();

    public IReadOnlyList<ShowcaseProductOptionViewModel> ProductOptions { get; set; } = Array.Empty<ShowcaseProductOptionViewModel>();
    public List<int> SelectedFeaturedProductIds { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if ((BackgroundImageFile is null || BackgroundImageFile.Length == 0) && string.IsNullOrWhiteSpace(BackgroundImageUrl))
        {
            yield return new ValidationResult(
                "The Arkaplan Gorseli field is required.",
                new[] { nameof(BackgroundImageUrl) });
        }

        if (!string.IsNullOrWhiteSpace(Description) && Description.Length > MaxDescriptionLength)
        {
            yield return new ValidationResult(
                $"Aciklama en fazla {MaxDescriptionLength} karakter olabilir.",
                new[] { nameof(Description) });
        }
    }
}

public class ShowcaseCategoryOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ShowcaseProductOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();
}

public class ShowcaseBackgroundOptionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
}
