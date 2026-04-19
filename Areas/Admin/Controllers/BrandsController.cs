using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class BrandsController : AdminControllerBase
{
    private readonly IAdminBrandService _adminBrandService;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IAdminBrandService adminBrandService, ILogger<BrandsController> logger)
    {
        _adminBrandService = adminBrandService;
        _logger = logger;
    }

    [HttpGet("/admin/brands")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var model = await _adminBrandService.GetBrandsAsync(cancellationToken);
        model = ApplyFilters(model, q, status);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Brands/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/brands/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminBrandService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/brands/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetValidationToast("Marka kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminBrandService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Marka kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Marka kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka olusturma sirasinda beklenmedik hata.");
            SetUnexpectedErrorToast("Marka kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/brands/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminBrandService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/brands/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BrandFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            SetValidationToast("Marka kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminBrandService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Marka kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka guncelleme sirasinda beklenmedik hata. BrandId: {BrandId}", id);
            SetUnexpectedErrorToast("Marka kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Marka kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    private static BrandListViewModel ApplyFilters(BrandListViewModel model, string? q, string? status)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();

        IEnumerable<BrandListItemViewModel> query = model.Brands;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(item.Description) && item.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        query = normalizedStatus switch
        {
            "active" => query.Where(item => item.IsActive),
            "passive" => query.Where(item => !item.IsActive),
            _ => query
        };

        var items = query.ToList();

        return new BrandListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            TotalCount = items.Count,
            ActiveCount = items.Count(item => item.IsActive),
            UsedCount = items.Count(item => item.ProductCount > 0),
            Brands = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
