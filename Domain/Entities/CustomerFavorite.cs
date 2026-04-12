namespace vitacure.Domain.Entities;

public class CustomerFavorite
{
    public int AppUserId { get; set; }
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? AppUser { get; set; }
    public Product? Product { get; set; }
}
