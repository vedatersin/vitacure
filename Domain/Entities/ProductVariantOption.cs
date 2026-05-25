namespace vitacure.Domain.Entities;

public class ProductVariantOption
{
    public int Id { get; set; }
    public int ProductVariantGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? SwatchImageUrl { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ProductVariantGroup? ProductVariantGroup { get; set; }
    public ICollection<ProductVariantSelection> VariantSelections { get; set; } = new List<ProductVariantSelection>();
}
