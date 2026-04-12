using vitacure.Models.ViewModels.Cart;

namespace vitacure.Application.Abstractions;

public interface ICartService
{
    Task<int> GetCartItemCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartViewModel?> GetCartAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartMutationResultViewModel> AddItemAsync(int userId, string productSlug, int quantity = 1, CancellationToken cancellationToken = default);
    Task<CartMutationResultViewModel> UpdateQuantityAsync(int userId, string productSlug, int quantity, CancellationToken cancellationToken = default);
    Task<CartMutationResultViewModel> RemoveItemAsync(int userId, string productSlug, CancellationToken cancellationToken = default);
}
