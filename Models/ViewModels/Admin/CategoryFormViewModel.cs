using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Üst Kategori")]
    public int? ParentId { get; set; }

    [Display(Name = "Görsel URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Sıralama Ölçütü")]
    public string? ProductSortType { get; set; }

    [Display(Name = "SEO Başlığı")]
    public string? SeoTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<CategoryOptionViewModel> ParentOptions { get; set; } = Array.Empty<CategoryOptionViewModel>();
    public IReadOnlyList<string> SortOptions { get; set; } = new[]
    {
        "One Cikanlar",
        "En Yeniler",
        "Fiyat Artan",
        "Fiyat Azalan",
        "En Cok Satanlar",
        "Puana Gore"
    };
}

public class CategoryOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
