using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class CollectionsController : AdminControllerBase
{
    private readonly IAdminCollectionService _adminCollectionService;
    private readonly ILogger<CollectionsController> _logger;

    public CollectionsController(IAdminCollectionService adminCollectionService, ILogger<CollectionsController> logger)
    {
        _adminCollectionService = adminCollectionService;
        _logger = logger;
    }

    [HttpGet("/admin/collections")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? home, CancellationToken cancellationToken)
    {
        CollectionListViewModel model;
        try
        {
            model = await _adminCollectionService.GetCollectionsAsync(cancellationToken);
            model = ApplyFilters(model, q, status, home);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Koleksiyon listesi yuklenirken beklenmedik hata olustu.");
            model = ApplyFilters(new CollectionListViewModel(), q, status, home);
            SetUnexpectedErrorToast("Koleksiyon listesi yuklenemedi", ex);
        }

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Collections/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/collections/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try
        {
            var model = await _adminCollectionService.GetCreateModelAsync(cancellationToken);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Koleksiyon olusturma formu yuklenirken beklenmedik hata olustu.");
            SetRedirectToast("error", "Koleksiyon formu acilamadi", "Koleksiyon olusturma ekrani yuklenemedi.", new[] { ex.Message.Trim() });
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("/admin/collections/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CollectionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetValidationToast("Koleksiyon kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminCollectionService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Koleksiyon kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetValidationToast("Koleksiyon kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Koleksiyon olusturma sirasinda beklenmedik hata.");
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetUnexpectedErrorToast("Koleksiyon kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/collections/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _adminCollectionService.GetEditModelAsync(id, cancellationToken);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Koleksiyon duzenleme formu yuklenirken beklenmedik hata olustu. CollectionId: {CollectionId}", id);
            SetRedirectToast("error", "Koleksiyon formu acilamadi", "Duzenleme ekrani yuklenemedi.", new[] { ex.Message.Trim() });
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("/admin/collections/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CollectionFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetValidationToast("Koleksiyon kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminCollectionService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetValidationToast("Koleksiyon kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Koleksiyon guncelleme sirasinda beklenmedik hata. CollectionId: {CollectionId}", id);
            model.ProductOptions = (await _adminCollectionService.GetCreateModelAsync(cancellationToken)).ProductOptions;
            SetUnexpectedErrorToast("Koleksiyon kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Koleksiyon kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    private static CollectionListViewModel ApplyFilters(CollectionListViewModel model, string? q, string? status, string? home)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var normalizedHome = string.IsNullOrWhiteSpace(home) ? "all" : home.Trim().ToLowerInvariant();

        IEnumerable<CollectionListItemViewModel> query = model.Collections;

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

        query = normalizedHome switch
        {
            "home" => query.Where(item => item.ShowOnHome),
            "hidden" => query.Where(item => !item.ShowOnHome),
            _ => query
        };

        var items = query.ToList();

        return new CollectionListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            HomeFilter = normalizedHome,
            TotalCount = items.Count,
            ActiveCount = items.Count(item => item.IsActive),
            HomeVisibleCount = items.Count(item => item.ShowOnHome),
            Collections = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
