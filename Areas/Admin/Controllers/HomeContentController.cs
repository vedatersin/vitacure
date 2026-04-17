using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class HomeContentController : Controller
{
    private readonly IAdminHomeContentService _adminHomeContentService;

    public HomeContentController(IAdminHomeContentService adminHomeContentService)
    {
        _adminHomeContentService = adminHomeContentService;
    }

    [HttpGet("/admin/home-content")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminHomeContentService.GetModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/home-content")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HomeContentFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _adminHomeContentService.UpdateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
