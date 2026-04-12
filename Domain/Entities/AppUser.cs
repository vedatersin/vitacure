using Microsoft.AspNetCore.Identity;
using vitacure.Domain.Enums;

namespace vitacure.Domain.Entities;

public class AppUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CustomerFavorite> Favorites { get; set; } = new List<CustomerFavorite>();
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<CustomerCartItem> CartItems { get; set; } = new List<CustomerCartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
