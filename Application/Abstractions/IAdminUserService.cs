using vitacure.Models.ViewModels.Admin;

namespace vitacure.Application.Abstractions;

public interface IAdminUserService
{
    Task<UserListViewModel> GetUsersAsync(CancellationToken cancellationToken = default);
}
