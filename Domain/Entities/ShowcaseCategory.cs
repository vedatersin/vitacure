namespace vitacure.Domain.Entities;

public class ShowcaseCategory
{
    public int ShowcaseId { get; set; }
    public int CategoryId { get; set; }

    public Showcase? Showcase { get; set; }
    public Category? Category { get; set; }
}
