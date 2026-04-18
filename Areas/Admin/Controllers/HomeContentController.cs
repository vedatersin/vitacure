using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class HomeContentController : AdminControllerBase
{
    private readonly IAdminHomeContentService _adminHomeContentService;
    private readonly ILogger<HomeContentController> _logger;

    public HomeContentController(IAdminHomeContentService adminHomeContentService, ILogger<HomeContentController> logger)
    {
        _adminHomeContentService = adminHomeContentService;
        _logger = logger;
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
            SetValidationToast("Ana sayfa ayarlari guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminHomeContentService.UpdateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla guncellendi", "Ana sayfa ayarlari guncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ana sayfa ayarlari guncellenirken beklenmedik hata.");
            SetUnexpectedErrorToast("Ana sayfa ayarlari guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
