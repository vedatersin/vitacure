namespace vitacure.Domain.Entities;

public class ProductBundleItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int ChildProductId { get; set; }
    public int? ChildProductVariantId { get; set; }
    public string EntryMode { get; set; } = "product";
    public int Quantity { get; set; } = 1;
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Product? ChildProduct { get; set; }
    public ProductVariant? ChildProductVariant { get; set; }
}
