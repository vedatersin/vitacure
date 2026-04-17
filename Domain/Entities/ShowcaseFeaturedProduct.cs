namespace vitacure.Domain.Entities;

public class ShowcaseFeaturedProduct
{
    public int ShowcaseId { get; set; }
    public int ProductId { get; set; }
    public int SortOrder { get; set; }

    public Showcase? Showcase { get; set; }
    public Product? Product { get; set; }
}
