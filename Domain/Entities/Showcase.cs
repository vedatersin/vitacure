namespace vitacure.Domain.Entities;

public class Showcase
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TagsContent { get; set; } = string.Empty;
    public string ExamplePromptsContent { get; set; } = string.Empty;
    public string BackgroundImageUrl { get; set; } = string.Empty;
    public int? PrimaryCategoryId { get; set; }
    public bool IsDark { get; set; } = true;
    public string? SeoTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowOnHome { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category? PrimaryCategory { get; set; }
    public ICollection<ShowcaseCategory> ShowcaseCategories { get; set; } = new List<ShowcaseCategory>();
    public ICollection<ShowcaseFeaturedProduct> FeaturedProducts { get; set; } = new List<ShowcaseFeaturedProduct>();
    public ICollection<ShowcasePrompt> Prompts { get; set; } = new List<ShowcasePrompt>();
    public ICollection<ShowcaseTag> Tags { get; set; } = new List<ShowcaseTag>();
}
