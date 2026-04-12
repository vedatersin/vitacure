using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;

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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminUserService.GetUsersAsync(cancellationToken);
        return View(model);
    }
}
