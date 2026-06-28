using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class UsersController : Controller
{
    private readonly IAdminUserService _adminUserService;

    public UsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("/admin/users")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? accountType, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var model = await _adminUserService.GetUsersAsync(cancellationToken);
        model = ApplyFilters(model, q, accountType, status);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Users/_ListContent.cshtml", model);
        }

        return View(model);
    }

    private static UserListViewModel ApplyFilters(UserListViewModel model, string? q, string? accountType, string? status)
    {
        var search = q?.Trim();
        var normalizedAccountType = string.IsNullOrWhiteSpace(accountType) ? "all" : accountType.Trim().ToLowerInvariant();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();

        IEnumerable<UserListItemViewModel> query = model.Users;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.RoleSummary.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedAccountType switch
        {
            "customer" => query.Where(item => item.AccountTypeLabel is "Müşteri" or "Musteri"),
            "backoffice" => query.Where(item => item.AccountTypeLabel is "Yönetim" or "Yonetim"),
            _ => query
        };

        query = normalizedStatus switch
        {
            "active" => query.Where(item => item.IsActive),
            "passive" => query.Where(item => !item.IsActive),
            _ => query
        };

        var items = query.ToList();

        return new UserListViewModel
        {
            SearchTerm = search,
            AccountTypeFilter = normalizedAccountType,
            StatusFilter = normalizedStatus,
            TotalCount = items.Count,
            CustomerCount = items.Count(item => item.AccountTypeLabel is "Müşteri" or "Musteri"),
            BackOfficeCount = items.Count(item => item.AccountTypeLabel is "Yönetim" or "Yonetim"),
            Users = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
