using vitacure.Domain.Enums;

namespace vitacure.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public ProductKind ProductKind { get; set; } = ProductKind.Physical;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? GalleryImageUrls { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal? Desi { get; set; }
    public string? HsCode { get; set; }
    public string? SupplierName { get; set; }
    public string? VariantFieldVisibilityJson { get; set; }
    public string? BundleMode { get; set; }
    public string? BundlePricingMode { get; set; }
    public string? BundleAdjustmentType { get; set; }
    public decimal? BundleAdjustmentAmount { get; set; }
    public int? BundleTotalQuantity { get; set; }
    public bool ContinueSellingWhenOutOfStock { get; set; }
    public bool ShowUnitPrice { get; set; }
    public decimal? UnitContentAmount { get; set; }
    public string? UnitContentType { get; set; }
    public decimal? UnitComparisonAmount { get; set; }
    public string? UnitComparisonType { get; set; }
    public int? BrandId { get; set; }
    public int? CategoryId { get; set; }
    public int? GoogleProductCategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ProductPublishingStatus Status { get; set; } = ProductPublishingStatus.PublishedOpen;
    public bool IsActive { get; set; } = true;

    public Brand? Brand { get; set; }
    public Category? Category { get; set; }
    public GoogleProductCategory? GoogleProductCategory { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ProductCollection> ProductCollections { get; set; } = new List<ProductCollection>();
    public ICollection<ProductCustomField> ProductCustomFields { get; set; } = new List<ProductCustomField>();
    public ICollection<ProductFeature> ProductFeatures { get; set; } = new List<ProductFeature>();
    public ICollection<ProductMedia> ProductMedias { get; set; } = new List<ProductMedia>();
    public ICollection<ProductPersonalization> ProductPersonalizations { get; set; } = new List<ProductPersonalization>();
    public ICollection<ProductBundleItem> ProductBundleItems { get; set; } = new List<ProductBundleItem>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<ProductVariantGroup> ProductVariantGroups { get; set; } = new List<ProductVariantGroup>();
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<ShowcaseFeaturedProduct> ShowcaseFeaturedProducts { get; set; } = new List<ShowcaseFeaturedProduct>();
}
