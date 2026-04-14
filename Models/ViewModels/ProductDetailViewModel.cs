namespace vitacure.Models.ViewModels;

public class ProductDetailViewModel
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalPath { get; set; } = "/";
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public IReadOnlyList<string> GalleryImages { get; set; } = Array.Empty<string>();
    public string Price { get; set; } = string.Empty;
    public string OldPrice { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
    public string RatingWidth { get; set; } = "0%";
    public string SizeLabel { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string AddToCartLabel { get; set; } = "Sepete Ekle";
    public string CartProductSlug { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = Array.Empty<BreadcrumbItemViewModel>();
    public IReadOnlyList<ProductCardViewModel> RelatedProducts { get; set; } = Array.Empty<ProductCardViewModel>();
}
