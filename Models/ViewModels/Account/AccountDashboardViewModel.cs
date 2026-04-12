using System.ComponentModel.DataAnnotations;
using vitacure.Models.ViewModels;

namespace vitacure.Models.ViewModels.Account;

public class AccountDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int FavoriteCount { get; set; }
    public int AddressCount { get; set; }
    public int OrderCount { get; set; }
    public IReadOnlyList<ProductCardViewModel> FavoriteProducts { get; set; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyList<AccountAddressViewModel> Addresses { get; set; } = Array.Empty<AccountAddressViewModel>();
    public IReadOnlyList<AccountOrderSummaryViewModel> Orders { get; set; } = Array.Empty<AccountOrderSummaryViewModel>();
    public AddressFormViewModel NewAddress { get; set; } = new();
}

public class AccountAddressViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}

public class AddressFormViewModel
{
    [Required]
    [Display(Name = "Adres Başlığı")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Teslim Alacak Kişi")]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "İl")]
    public string City { get; set; } = string.Empty;

    [Required]
    [Display(Name = "İlçe")]
    public string District { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Adres")]
    public string AddressLine { get; set; } = string.Empty;

    [Display(Name = "Posta Kodu")]
    public string? PostalCode { get; set; }

    [Display(Name = "Varsayılan Adres")]
    public bool IsDefault { get; set; }
}

public class FavoriteToggleResultViewModel
{
    public bool IsFavorite { get; set; }
    public int FavoriteCount { get; set; }
}

public class AccountOrderSummaryViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TotalAmount { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<AccountOrderItemViewModel> Items { get; set; } = Array.Empty<AccountOrderItemViewModel>();
}

public class AccountOrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string LineTotal { get; set; } = string.Empty;
}

public class OrderPlacementResultViewModel
{
    public bool IsSuccess { get; set; }
    public int? OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
