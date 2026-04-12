namespace vitacure.Models.ViewModels.Cart;

public class CartMutationResultViewModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CartItemCount { get; set; }
    public int? ItemQuantity { get; set; }
    public decimal? CartTotalAmountValue { get; set; }
    public string? CartTotalAmount { get; set; }
}
