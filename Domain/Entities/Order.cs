using vitacure.Domain.Enums;

namespace vitacure.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int AppUserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? AppUser { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
