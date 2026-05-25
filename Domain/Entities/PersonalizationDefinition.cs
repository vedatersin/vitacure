namespace vitacure.Domain.Entities;

public class PersonalizationDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string InputType { get; set; } = "Text";
    public bool IsActive { get; set; } = true;

    public ICollection<ProductPersonalization> ProductPersonalizations { get; set; } = new List<ProductPersonalization>();
}
