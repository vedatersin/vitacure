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
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Category? Category { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public ICollection<ShowcaseFeaturedProduct> ShowcaseFeaturedProducts { get; set; } = new List<ShowcaseFeaturedProduct>();
}
