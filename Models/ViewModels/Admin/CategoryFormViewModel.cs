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

    [Display(Name = "SEO Başlığı")]
    public string? SeoTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<CategoryOptionViewModel> ParentOptions { get; set; } = Array.Empty<CategoryOptionViewModel>();
}

public class CategoryOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
