using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminOrderService
{
    Task<AdminOrderListViewModel> GetOrdersAsync(CancellationToken cancellationToken = default);
}
