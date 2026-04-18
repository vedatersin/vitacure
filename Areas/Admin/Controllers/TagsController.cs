using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class TagsController : AdminControllerBase
{
    private readonly IAdminTagService _adminTagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(IAdminTagService adminTagService, ILogger<TagsController> logger)
    {
        _adminTagService = adminTagService;
        _logger = logger;
    }

    [HttpGet("/admin/tags")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? usage, CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetTagsAsync(cancellationToken);
        model = ApplyFilters(model, q, usage);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Tags/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/tags/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/tags/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetValidationToast("Etiket kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminTagService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Etiket kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Etiket kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket olusturma sirasinda beklenmedik hata.");
            SetUnexpectedErrorToast("Etiket kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/tags/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/tags/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TagFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            SetValidationToast("Etiket kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminTagService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            SetValidationToast("Etiket kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket guncelleme sirasinda beklenmedik hata. TagId: {TagId}", id);
            SetUnexpectedErrorToast("Etiket kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Etiket kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    private static TagListViewModel ApplyFilters(TagListViewModel model, string? q, string? usage)
    {
        var search = q?.Trim();
        var normalizedUsage = string.IsNullOrWhiteSpace(usage) ? "all" : usage.Trim().ToLowerInvariant();

        IEnumerable<TagListItemViewModel> query = model.Tags;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedUsage switch
        {
            "used" => query.Where(item => item.ProductCount > 0),
            "unused" => query.Where(item => item.ProductCount <= 0),
            _ => query
        };

        var items = query.ToList();

        return new TagListViewModel
        {
            SearchTerm = search,
            UsageFilter = normalizedUsage,
            TotalCount = items.Count,
            UsedCount = items.Count(item => item.ProductCount > 0),
            Tags = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
