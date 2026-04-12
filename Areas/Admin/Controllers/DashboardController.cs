using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class DashboardController : Controller
{
    private readonly IAdminDashboardService _adminDashboardService;

    public DashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("/admin/dashboard")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminDashboardService.GetDashboardAsync(cancellationToken);
        return View(model);
    }
}
