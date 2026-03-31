namespace vitacure.Models.ViewModels;

public class ProductCardViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string OldPrice { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
    public string RatingWidth { get; set; } = "0%";
    public string Description { get; set; } = string.Empty;
    public string Href { get; set; } = "#";
    public string FavoriteTitle { get; set; } = "Favorilere Ekle";
    public string AddToCartLabel { get; set; } = "Sepete Ekle";
}
