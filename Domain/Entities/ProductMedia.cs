namespace vitacure.Domain.Entities;

public class ProductMedia
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? MediaAssetId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public MediaAsset? MediaAsset { get; set; }
}
