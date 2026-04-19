namespace vitacure.Models.ViewModels.Cart;

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = Array.Empty<CartItemViewModel>();
    public int TotalQuantity { get; set; }
    public decimal TotalAmountValue { get; set; }
    public string TotalAmount { get; set; } = "0,00";
    public bool IsEmpty => Items.Count == 0;
}

public class CartItemViewModel
{
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int? VariantId { get; set; }
    public string? VariantLabel { get; set; }
    public string ProductImageUrl { get; set; } = string.Empty;
    public string ProductHref { get; set; } = "#";
    public int Quantity { get; set; }
    public decimal UnitPriceValue { get; set; }
    public string UnitPrice { get; set; } = "0,00";
    public decimal LineTotalValue { get; set; }
    public string LineTotal { get; set; } = "0,00";
}
