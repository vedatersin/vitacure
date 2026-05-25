namespace vitacure.Domain.Entities;

public class CustomFieldDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text";
    public bool IsFilterable { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductCustomField> ProductCustomFields { get; set; } = new List<ProductCustomField>();
}
