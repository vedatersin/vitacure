namespace vitacure.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? GalleryImageUrls { get; set; }
    public int Stock { get; set; }
    public int? BrandId { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Brand? Brand { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ProductCollection> ProductCollections { get; set; } = new List<ProductCollection>();
    public ICollection<ProductFeature> ProductFeatures { get; set; } = new List<ProductFeature>();
    public ICollection<ProductMedia> ProductMedias { get; set; } = new List<ProductMedia>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<ShowcaseFeaturedProduct> ShowcaseFeaturedProducts { get; set; } = new List<ShowcaseFeaturedProduct>();
}
