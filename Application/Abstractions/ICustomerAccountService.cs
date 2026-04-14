using vitacure.Models.ViewModels.Account;

namespace vitacure.Application.Abstractions;

public interface ICustomerAccountService
{
    Task<AccountDashboardViewModel?> GetDashboardAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFavoriteProductSlugsAsync(int userId, CancellationToken cancellationToken = default);
    Task<FavoriteToggleResultViewModel> ToggleFavoriteAsync(int userId, string productSlug, CancellationToken cancellationToken = default);
    Task<bool> AddAddressAsync(int userId, AddressFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateAddressAsync(int userId, int addressId, AddressFormViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int userId, ProfileFormViewModel model, CancellationToken cancellationToken = default);
    Task<int> GetOrderCountAsync(int userId, CancellationToken cancellationToken = default);
}
