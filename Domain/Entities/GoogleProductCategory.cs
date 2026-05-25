namespace vitacure.Domain.Entities;

public class GoogleProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public GoogleProductCategory? Parent { get; set; }
    public ICollection<GoogleProductCategory> Children { get; set; } = new List<GoogleProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
