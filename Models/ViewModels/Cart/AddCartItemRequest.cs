namespace vitacure.Models.ViewModels.Cart;

public class AddCartItemRequest
{
    public string ProductSlug { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
