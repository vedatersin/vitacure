using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class ShowcasesController : AdminControllerBase
{
    private readonly IAdminShowcaseService _adminShowcaseService;
    private readonly ILogger<ShowcasesController> _logger;

    public ShowcasesController(IAdminShowcaseService adminShowcaseService, ILogger<ShowcasesController> logger)
    {
        _adminShowcaseService = adminShowcaseService;
        _logger = logger;
    }

    [HttpGet("/admin/showcases")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? home, CancellationToken cancellationToken)
    {
        var model = await _adminShowcaseService.GetShowcasesAsync(cancellationToken);
        model = ApplyFilters(model, q, status, home);
        return View(model);
    }

    [HttpGet("/admin/showcases/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminShowcaseService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/showcases/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShowcaseFormViewModel model, CancellationToken cancellationToken)
    {
        ApplyIsDarkFromRequest(model);

        if (!ModelState.IsValid)
        {
            var refill = await _adminShowcaseService.GetCreateModelAsync(cancellationToken);
            model.BackgroundOptions = refill.BackgroundOptions;
            if (string.IsNullOrWhiteSpace(model.BackgroundImageUrl))
            {
                model.BackgroundImageUrl = refill.BackgroundImageUrl;
            }
            model.CategoryOptions = refill.CategoryOptions;
            model.ProductOptions = refill.ProductOptions;
            SetValidationToast("Vitrin kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminShowcaseService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", $"Vitrin kaydi olusturuldu. Kaydedilen mod: {(model.IsDark ? "Dark" : "Light")}.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var refill = await _adminShowcaseService.GetCreateModelAsync(cancellationToken);
            model.BackgroundOptions = refill.BackgroundOptions;
            if (string.IsNullOrWhiteSpace(model.BackgroundImageUrl))
            {
                model.BackgroundImageUrl = refill.BackgroundImageUrl;
            }
            model.CategoryOptions = refill.CategoryOptions;
            model.ProductOptions = refill.ProductOptions;
            SetValidationToast("Vitrin kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vitrin olusturma sirasinda beklenmedik hata. Slug: {Slug}", model.Slug);
            var refill = await _adminShowcaseService.GetCreateModelAsync(cancellationToken);
            model.BackgroundOptions = refill.BackgroundOptions;
            if (string.IsNullOrWhiteSpace(model.BackgroundImageUrl))
            {
                model.BackgroundImageUrl = refill.BackgroundImageUrl;
            }
            model.CategoryOptions = refill.CategoryOptions;
            model.ProductOptions = refill.ProductOptions;
            SetUnexpectedErrorToast("Vitrin kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/showcases/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminShowcaseService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/showcases/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ShowcaseFormViewModel model, CancellationToken cancellationToken)
    {
        ApplyIsDarkFromRequest(model);

        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var refill = await _adminShowcaseService.GetEditModelAsync(id, cancellationToken);
            model.BackgroundOptions = refill?.BackgroundOptions ?? Array.Empty<ShowcaseBackgroundOptionViewModel>();
            model.CategoryOptions = refill?.CategoryOptions ?? Array.Empty<ShowcaseCategoryOptionViewModel>();
            model.ProductOptions = refill?.ProductOptions ?? Array.Empty<ShowcaseProductOptionViewModel>();
            SetValidationToast("Vitrin kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminShowcaseService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var refill = await _adminShowcaseService.GetEditModelAsync(id, cancellationToken);
            model.BackgroundOptions = refill?.BackgroundOptions ?? Array.Empty<ShowcaseBackgroundOptionViewModel>();
            model.CategoryOptions = refill?.CategoryOptions ?? Array.Empty<ShowcaseCategoryOptionViewModel>();
            model.ProductOptions = refill?.ProductOptions ?? Array.Empty<ShowcaseProductOptionViewModel>();
            SetValidationToast("Vitrin kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vitrin guncelleme sirasinda beklenmedik hata. ShowcaseId: {ShowcaseId}, RequestedMode: {RequestedMode}", id, model.IsDark);
            var refill = await _adminShowcaseService.GetEditModelAsync(id, cancellationToken);
            model.BackgroundOptions = refill?.BackgroundOptions ?? Array.Empty<ShowcaseBackgroundOptionViewModel>();
            model.CategoryOptions = refill?.CategoryOptions ?? Array.Empty<ShowcaseCategoryOptionViewModel>();
            model.ProductOptions = refill?.ProductOptions ?? Array.Empty<ShowcaseProductOptionViewModel>();
            SetUnexpectedErrorToast("Vitrin kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", $"Vitrin kaydi guncellendi. Kaydedilen mod: {(model.IsDark ? "Dark" : "Light")}.");
        return RedirectToAction(nameof(Index));
    }

    private void ApplyIsDarkFromRequest(ShowcaseFormViewModel model)
    {
        if (!Request.HasFormContentType)
        {
            return;
        }

        var rawValue = Request.Form["IsDark"].ToString();
        if (bool.TryParse(rawValue, out var parsedValue))
        {
            model.IsDark = parsedValue;
        }

        _logger.LogInformation("Showcase form post IsDark raw value: {IsDarkRaw}, parsed: {IsDarkParsed}, showcaseId: {ShowcaseId}", rawValue, model.IsDark, model.Id);
    }

    private static ShowcaseListViewModel ApplyFilters(ShowcaseListViewModel model, string? q, string? status, string? home)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var normalizedHome = string.IsNullOrWhiteSpace(home) ? "all" : home.Trim().ToLowerInvariant();

        IEnumerable<ShowcaseListItemViewModel> query = model.Showcases;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase));
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

        return new ShowcaseListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            HomeFilter = normalizedHome,
            TotalCount = items.Count,
            ActiveCount = items.Count(item => item.IsActive),
            HomeVisibleCount = items.Count(item => item.ShowOnHome),
            Showcases = items
        };
    }
}
