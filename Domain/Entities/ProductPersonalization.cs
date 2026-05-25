namespace vitacure.Domain.Entities;

public class ProductPersonalization
{
    public int ProductId { get; set; }
    public int PersonalizationDefinitionId { get; set; }

    public Product? Product { get; set; }
    public PersonalizationDefinition? PersonalizationDefinition { get; set; }
}
