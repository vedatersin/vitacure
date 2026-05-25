namespace vitacure.Domain.Entities;

public class ProductCustomField
{
    public int ProductId { get; set; }
    public int CustomFieldDefinitionId { get; set; }

    public Product? Product { get; set; }
    public CustomFieldDefinition? CustomFieldDefinition { get; set; }
}
