using vitacure.Models.ViewModels.Account;
using vitacure.Models.ViewModels.Cart;

namespace vitacure.Application.Abstractions;

public interface IGuestSessionService
{
    IReadOnlyList<string> GetFavoriteProductSlugs();
    int GetFavoriteCount();
    FavoriteToggleResultViewModel ToggleFavorite(string productSlug);
    Task<CartViewModel> GetCartAsync(CancellationToken cancellationToken = default);
    int GetCartItemCount();
    Task<CartMutationResultViewModel> AddCartItemAsync(string productSlug, int quantity = 1, int? variantId = null, CancellationToken cancellationToken = default);
    Task<CartMutationResultViewModel> UpdateCartQuantityAsync(string productSlug, int quantity, int? variantId = null, CancellationToken cancellationToken = default);
    Task<CartMutationResultViewModel> RemoveCartItemAsync(string productSlug, int? variantId = null, CancellationToken cancellationToken = default);
    Task<bool> MergeIntoCustomerAccountAsync(int userId, CancellationToken cancellationToken = default);
}
