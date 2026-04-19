namespace vitacure.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SeoTitle { get; set; }
    public string? MetaDescription { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ShowcaseCategory> ShowcaseCategories { get; set; } = new List<ShowcaseCategory>();
}
