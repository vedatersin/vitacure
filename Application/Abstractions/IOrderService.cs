using vitacure.Models.ViewModels.Account;

namespace vitacure.Application.Abstractions;

public interface IOrderService
{
    Task<IReadOnlyList<AccountOrderSummaryViewModel>> GetOrderHistoryAsync(int userId, CancellationToken cancellationToken = default);
    Task<OrderPlacementResultViewModel> PlaceOrderFromCartAsync(int userId, CancellationToken cancellationToken = default);
}
