using Microsoft.EntityFrameworkCore;
using vitacure.Application.Abstractions;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _dbContext;

    public AdminUserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserListViewModel> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var userRolePairs = await _dbContext.UserRoles
            .AsNoTracking()
            .Join(
                _dbContext.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, role.Name })
            .ToListAsync(cancellationToken);

        var rolesByUserId = userRolePairs
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => string.Join(", ", x.Select(y => y.Name).Where(y => !string.IsNullOrWhiteSpace(y))));

        var items = users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? "-" : user.FullName,
            Email = user.Email ?? "-",
            AccountTypeLabel = user.AccountType == AccountType.BackOffice ? "Yönetim" : "Müşteri",
            RoleSummary = rolesByUserId.TryGetValue(user.Id, out var roleSummary) && !string.IsNullOrWhiteSpace(roleSummary)
                ? roleSummary
                : "-",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();

        return new UserListViewModel
        {
            TotalCount = items.Count,
            CustomerCount = items.Count(x => x.AccountTypeLabel == "Müşteri"),
            BackOfficeCount = items.Count(x => x.AccountTypeLabel == "Yönetim"),
            Users = items
        };
    }
}
