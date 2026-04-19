using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class CollectionFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Koleksiyon Adi")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Aciklama")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Anasayfada Goster")]
    public bool ShowOnHome { get; set; }

    [Display(Name = "Sira")]
    [Range(0, 999)]
    public int SortOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<CollectionProductOptionViewModel> ProductOptions { get; set; } = Array.Empty<CollectionProductOptionViewModel>();
    public IReadOnlyList<int> SelectedProductIds { get; set; } = Array.Empty<int>();
}

public class CollectionProductOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
