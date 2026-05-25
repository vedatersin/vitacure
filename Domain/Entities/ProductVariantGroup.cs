namespace vitacure.Domain.Entities;

public class ProductVariantGroup
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SelectionStyle { get; set; } = "list";
    public bool ShowOnCard { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public ICollection<ProductVariantOption> Options { get; set; } = new List<ProductVariantOption>();
}
