namespace vitacure.Domain.Entities;

public class ProductVariantSelection
{
    public int ProductVariantId { get; set; }
    public int ProductVariantOptionId { get; set; }

    public ProductVariant? ProductVariant { get; set; }
    public ProductVariantOption? ProductVariantOption { get; set; }
}
