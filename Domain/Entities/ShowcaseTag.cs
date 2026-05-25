namespace vitacure.Domain.Entities;

public class ShowcaseTag
{
    public int Id { get; set; }
    public int ShowcaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Showcase? Showcase { get; set; }
}
