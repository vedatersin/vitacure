using System.ComponentModel.DataAnnotations;
using vitacure.Domain.Enums;

namespace vitacure.Models.ViewModels.Admin;

public class ProductFormViewModel : IValidatableObject
{
    public int? Id { get; set; }
    public string CreateMode { get; set; } = "simple";
    public ProductKind ProductKind { get; set; } = ProductKind.Physical;
    public string BundleMode { get; set; } = "simple";
    public string BundlePricingMode { get; set; } = "manual";
    public string BundleAdjustmentType { get; set; } = "none";
    public decimal? BundleAdjustmentAmount { get; set; }
    public int? BundleTotalQuantity { get; set; }

    [Display(Name = "Ürün Adi")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "SEO Basligi")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Display(Name = "Eski Fiyat")]
    public decimal? OldPrice { get; set; }

    [Display(Name = "Alis Fiyati")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Puan")]
    public decimal Rating { get; set; } = 5m;

    [Display(Name = "Degerlendirme Sayisi")]
    public int ReviewCount { get; set; }

    [Display(Name = "Görsel URL")]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Galeri Görselleri")]
    public string? GalleryImageUrls { get; set; }

    public string? MediaItemsJson { get; set; }

    [Display(Name = "Stok")]
    public int Stock { get; set; }

    [Display(Name = "SKU")]
    public string? Sku { get; set; }

    [Display(Name = "Barkod")]
    public string? Barcode { get; set; }

    [Display(Name = "Desi")]
    public decimal? Desi { get; set; }

    [Display(Name = "HS Kodu")]
    public string? HsCode { get; set; }

    [Display(Name = "Tedarikçi")]
    public string? SupplierName { get; set; }

    public ProductVariantFieldVisibilityViewModel VariantFieldVisibility { get; set; } = new();

    [Display(Name = "Birim Fiyat Goster")]
    public bool ShowUnitPrice { get; set; }

    [Display(Name = "Ürünün Birim Ölçüsü")]
    public decimal? UnitContentAmount { get; set; }

    [Display(Name = "Ürünün Birim Tipi")]
    public string? UnitContentType { get; set; }

    [Display(Name = "Satilan Birim")]
    public decimal? UnitComparisonAmount { get; set; }

    [Display(Name = "Satilan Birim Tipi")]
    public string? UnitComparisonType { get; set; }

    [Display(Name = "Stok Tukenince Satisa Devam Et")]
    public bool ContinueSellingWhenOutOfStock { get; set; }

    [Display(Name = "Marka")]
    public int? BrandId { get; set; }

    [Display(Name = "Google Ürün Kategorisi")]
    public int? GoogleProductCategoryId { get; set; }

    [Display(Name = "Kategori")]
    public int? CategoryId { get; set; }

    public IReadOnlyList<int> SelectedCategoryIds { get; set; } = Array.Empty<int>();

    [Display(Name = "Durum")]
    public ProductPublishingStatus Status { get; set; } = ProductPublishingStatus.Draft;

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ProductVariantInputViewModel> Variants { get; set; } = Array.Empty<ProductVariantInputViewModel>();
    public IReadOnlyList<ProductVariantGroupInputViewModel> VariantGroups { get; set; } = Array.Empty<ProductVariantGroupInputViewModel>();
    public IReadOnlyList<ProductVariantPresetViewModel> VariantPresets { get; set; } = Array.Empty<ProductVariantPresetViewModel>();
    public IReadOnlyList<ProductBundleItemInputViewModel> BundleItems { get; set; } = Array.Empty<ProductBundleItemInputViewModel>();
    public IReadOnlyList<ProductBundleProductOptionViewModel> BundleProductOptions { get; set; } = Array.Empty<ProductBundleProductOptionViewModel>();

    public IReadOnlyList<ProductBrandOptionViewModel> BrandOptions { get; set; } = Array.Empty<ProductBrandOptionViewModel>();
    public IReadOnlyList<ProductCategoryOptionViewModel> CategoryOptions { get; set; } = Array.Empty<ProductCategoryOptionViewModel>();
    public IReadOnlyList<ProductGoogleCategoryOptionViewModel> GoogleProductCategoryOptions { get; set; } = Array.Empty<ProductGoogleCategoryOptionViewModel>();
    public IReadOnlyList<ProductFeatureOptionViewModel> FeatureOptions { get; set; } = Array.Empty<ProductFeatureOptionViewModel>();
    public IReadOnlyList<ProductCustomFieldOptionViewModel> CustomFieldOptions { get; set; } = Array.Empty<ProductCustomFieldOptionViewModel>();
    public IReadOnlyList<ProductPersonalizationOptionViewModel> PersonalizationOptions { get; set; } = Array.Empty<ProductPersonalizationOptionViewModel>();
    public IReadOnlyList<ProductTagOptionViewModel> TagOptions { get; set; } = Array.Empty<ProductTagOptionViewModel>();
    public Dictionary<int, string> SelectedFeatureValues { get; set; } = new();
    public IReadOnlyList<int> SelectedFeatureIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> SelectedCustomFieldIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> SelectedPersonalizationIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> SelectedTagIds { get; set; } = Array.Empty<int>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReviewCount < 0)
        {
            yield return new ValidationResult("Degerlendirme sayisi sifirdan kucuk olamaz.", new[] { nameof(ReviewCount) });
        }

        if (Rating is < 0 or > 5)
        {
            yield return new ValidationResult("Yildiz puani 0 ile 5 arasinda olmalidir.", new[] { nameof(Rating) });
        }

        if (Stock < 0)
        {
            yield return new ValidationResult("Stok sifirdan kucuk olamaz.", new[] { nameof(Stock) });
        }

        if (Price < 0)
        {
            yield return new ValidationResult("Satis fiyati sifirdan kucuk olamaz.", new[] { nameof(Price) });
        }

        if (OldPrice is < 0)
        {
            yield return new ValidationResult("Indirimli fiyat sifirdan kucuk olamaz.", new[] { nameof(OldPrice) });
        }

        if (PurchasePrice is < 0)
        {
            yield return new ValidationResult("Alis fiyati sifirdan kucuk olamaz.", new[] { nameof(PurchasePrice) });
        }

        if (Desi is < 0)
        {
            yield return new ValidationResult("Desi sifirdan kucuk olamaz.", new[] { nameof(Desi) });
        }

        if (UnitContentAmount is < 0)
        {
            yield return new ValidationResult("Ürünün birim olcusu sifirdan kucuk olamaz.", new[] { nameof(UnitContentAmount) });
        }

        if (UnitComparisonAmount is < 0)
        {
            yield return new ValidationResult("Satilan birim sifirdan kucuk olamaz.", new[] { nameof(UnitComparisonAmount) });
        }

        if (BundleAdjustmentAmount is < 0)
        {
            yield return new ValidationResult("Paket fiyat ayari sifirdan kucuk olamaz.", new[] { nameof(BundleAdjustmentAmount) });
        }

        if (BundleTotalQuantity is < 0)
        {
            yield return new ValidationResult("Toplam adet sifirdan kucuk olamaz.", new[] { nameof(BundleTotalQuantity) });
        }

        if (ShowUnitPrice)
        {
            if (UnitContentAmount is null or <= 0)
            {
                yield return new ValidationResult("Ürünün birim olcusunu girmelisiniz.", new[] { nameof(UnitContentAmount) });
            }

            if (string.IsNullOrWhiteSpace(UnitContentType))
            {
                yield return new ValidationResult("Ürünün birim tipini secmelisiniz.", new[] { nameof(UnitContentType) });
            }

            if (UnitComparisonAmount is null or <= 0)
            {
                yield return new ValidationResult("Satilan birimi girmelisiniz.", new[] { nameof(UnitComparisonAmount) });
            }

            if (string.IsNullOrWhiteSpace(UnitComparisonType))
            {
                yield return new ValidationResult("Satilan birim tipini secmelisiniz.", new[] { nameof(UnitComparisonType) });
            }
        }

        if (Status.AllowsIncompleteSave())
        {
            yield break;
        }

        var isBundleMode = CreateMode is "bundle" or "bundle-variant" || ProductKind == ProductKind.Bundle;
        if (isBundleMode && BundleItems.Count == 0)
        {
            yield return new ValidationResult("Paket urun icin en az bir urun eklemelisiniz.", new[] { nameof(BundleItems) });
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Ürün adi zorunludur.", new[] { nameof(Name) });
        }

        if (string.IsNullOrWhiteSpace(Slug))
        {
            yield return new ValidationResult("Slug zorunludur.", new[] { nameof(Slug) });
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult("Açıklama zorunludur.", new[] { nameof(Description) });
        }

        if (string.IsNullOrWhiteSpace(ImageUrl))
        {
            yield return new ValidationResult("Ürün gorseli zorunludur.", new[] { nameof(ImageUrl) });
        }

        if (Price <= 0)
        {
            yield return new ValidationResult("Satis fiyati sifirdan buyuk olmalidir.", new[] { nameof(Price) });
        }

        if (!CategoryId.HasValue || CategoryId.Value <= 0)
        {
            yield return new ValidationResult("Kategori secmelisiniz.", new[] { nameof(CategoryId) });
        }
    }
}

public class ProductBundleItemInputViewModel
{
    public int? Id { get; set; }
    public int? ParentVariantId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public string EntryMode { get; set; } = "product";
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string? ProductVariantLabel { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int SortOrder { get; set; }
}

public class ProductBundleProductOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool HasVariants { get; set; }
    public IReadOnlyList<ProductBundleProductVariantOptionViewModel> Variants { get; set; } = Array.Empty<ProductBundleProductVariantOptionViewModel>();
}

public class ProductBundleProductVariantOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ProductVariantInputViewModel
{
    public int? Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int Stock { get; set; }
    public decimal? Desi { get; set; }
    public string? HsCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<int> OptionIds { get; set; } = Array.Empty<int>();
}

public class ProductVariantGroupInputViewModel
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SelectionStyle { get; set; } = "list";
    public bool ShowOnCard { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public IReadOnlyList<ProductVariantOptionInputViewModel> Options { get; set; } = Array.Empty<ProductVariantOptionInputViewModel>();
}

public class ProductVariantOptionInputViewModel
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? SwatchImageUrl { get; set; }
    public int SortOrder { get; set; }
}

public class ProductVariantPresetViewModel
{
    public string Name { get; set; } = string.Empty;
    public string SelectionStyle { get; set; } = "list";
    public IReadOnlyList<ProductVariantPresetOptionViewModel> Options { get; set; } = Array.Empty<ProductVariantPresetOptionViewModel>();
}

public class ProductVariantPresetOptionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? SwatchImageUrl { get; set; }
}

public class ProductVariantFieldVisibilityViewModel
{
    public bool ShowImage { get; set; } = true;
    public bool ShowBarcode { get; set; } = true;
    public bool ShowPurchasePrice { get; set; } = true;
    public bool ShowDesi { get; set; }
    public bool ShowHsCode { get; set; }
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

public class ProductGoogleCategoryOptionViewModel
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

public class ProductCustomFieldOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsFilterable { get; set; }
}

public class ProductPersonalizationOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
}

public class ProductFeatureOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? OptionsPreview { get; set; }
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
}
