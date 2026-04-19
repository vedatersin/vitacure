namespace vitacure.Domain.Entities;

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? OptionsContent { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductFeature> ProductFeatures { get; set; } = new List<ProductFeature>();
}
