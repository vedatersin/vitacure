using vitacure.Models.ViewModels.Account;

namespace vitacure.Application.Abstractions;

public interface ICustomerAccountService
{
    Task<AccountDashboardViewModel?> GetDashboardAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFavoriteProductSlugsAsync(int userId, CancellationToken cancellationToken = default);
    Task<FavoriteToggleResultViewModel> ToggleFavoriteAsync(int userId, string productSlug, CancellationToken cancellationToken = default);
    Task<bool> AddAddressAsync(int userId, AddressFormViewModel model, CancellationToken cancellationToken = default);
    Task<int> GetOrderCountAsync(int userId, CancellationToken cancellationToken = default);
}
