namespace vitacure.Domain.Entities;

public class ShowcasePrompt
{
    public int Id { get; set; }
    public int ShowcaseId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Showcase? Showcase { get; set; }
}
