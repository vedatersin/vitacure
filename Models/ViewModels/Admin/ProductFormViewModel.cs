using System.ComponentModel.DataAnnotations;

namespace vitacure.Models.ViewModels.Admin;

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01d, 9999999d)]
    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Display(Name = "Eski Fiyat")]
    public decimal? OldPrice { get; set; }

    [Range(0d, 5d)]
    [Display(Name = "Puan")]
    public decimal Rating { get; set; } = 5m;

    [Required]
    [Display(Name = "Görsel URL")]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Galeri Görselleri")]
    public string? GalleryImageUrls { get; set; }

    public string? MediaItemsJson { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stok")]
    public int Stock { get; set; }

    [Display(Name = "Marka")]
    public int? BrandId { get; set; }

    [Display(Name = "Kategori")]
    [Range(1, int.MaxValue, ErrorMessage = "Kategori seçmelisiniz.")]
    public int CategoryId { get; set; }

    public IReadOnlyList<int> SelectedCategoryIds { get; set; } = Array.Empty<int>();

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ProductVariantInputViewModel> Variants { get; set; } = Array.Empty<ProductVariantInputViewModel>();

    public IReadOnlyList<ProductBrandOptionViewModel> BrandOptions { get; set; } = Array.Empty<ProductBrandOptionViewModel>();
    public IReadOnlyList<ProductCategoryOptionViewModel> CategoryOptions { get; set; } = Array.Empty<ProductCategoryOptionViewModel>();
    public IReadOnlyList<ProductFeatureOptionViewModel> FeatureOptions { get; set; } = Array.Empty<ProductFeatureOptionViewModel>();
    public IReadOnlyList<ProductTagOptionViewModel> TagOptions { get; set; } = Array.Empty<ProductTagOptionViewModel>();
    public Dictionary<int, string> SelectedFeatureValues { get; set; } = new();
    public IReadOnlyList<int> SelectedFeatureIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> SelectedTagIds { get; set; } = Array.Empty<int>();
}

public class ProductVariantInputViewModel
{
    public int? Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductBrandOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductCategoryOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

public class ProductTagOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class ProductFeatureOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? OptionsPreview { get; set; }
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
}
