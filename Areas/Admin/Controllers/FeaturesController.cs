using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class FeaturesController : AdminControllerBase
{
    private readonly IAdminFeatureService _adminFeatureService;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(IAdminFeatureService adminFeatureService, ILogger<FeaturesController> logger)
    {
        _adminFeatureService = adminFeatureService;
        _logger = logger;
    }

    [HttpGet("/admin/features")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var model = await _adminFeatureService.GetFeaturesAsync(cancellationToken);
        model = ApplyFilters(model, q, status);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Features/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/features/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminFeatureService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/features/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FeatureFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetValidationToast("Ozellik kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminFeatureService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Ozellik kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Ozellik kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ozellik olusturma sirasinda beklenmedik hata.");
            SetUnexpectedErrorToast("Ozellik kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/features/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminFeatureService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/features/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FeatureFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            SetValidationToast("Ozellik kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminFeatureService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Ozellik kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ozellik guncelleme sirasinda beklenmedik hata. FeatureId: {FeatureId}", id);
            SetUnexpectedErrorToast("Ozellik kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Ozellik kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    private static FeatureListViewModel ApplyFilters(FeatureListViewModel model, string? q, string? status)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();

        IEnumerable<FeatureListItemViewModel> query = model.Features;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.GroupName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedStatus switch
        {
            "active" => query.Where(item => item.IsActive),
            "passive" => query.Where(item => !item.IsActive),
            _ => query
        };

        var items = query.ToList();

        return new FeatureListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            TotalCount = items.Count,
            ActiveCount = items.Count(item => item.IsActive),
            UsedCount = items.Count(item => item.ProductCount > 0),
            Features = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
