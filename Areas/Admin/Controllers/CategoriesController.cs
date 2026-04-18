using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class CategoriesController : AdminControllerBase
{
    private readonly IAdminCategoryService _adminCategoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IAdminCategoryService adminCategoryService, ILogger<CategoriesController> logger)
    {
        _adminCategoryService = adminCategoryService;
        _logger = logger;
    }

    [HttpGet("/admin/categories")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? structure, CancellationToken cancellationToken)
    {
        var model = await _adminCategoryService.GetCategoriesAsync(cancellationToken);
        model = ApplyFilters(model, q, status, structure);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Categories/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/categories/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminCategoryService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.ParentOptions = (await _adminCategoryService.GetCreateModelAsync(cancellationToken)).ParentOptions;
            SetValidationToast("Kategori kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminCategoryService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Kategori kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            model.ParentOptions = (await _adminCategoryService.GetCreateModelAsync(cancellationToken)).ParentOptions;
            SetValidationToast("Kategori kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori olusturma sirasinda beklenmedik hata.");
            model.ParentOptions = (await _adminCategoryService.GetCreateModelAsync(cancellationToken)).ParentOptions;
            SetUnexpectedErrorToast("Kategori kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/categories/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminCategoryService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/categories/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var editModel = await _adminCategoryService.GetEditModelAsync(id, cancellationToken);
            model.ParentOptions = editModel?.ParentOptions ?? Array.Empty<CategoryOptionViewModel>();
            SetValidationToast("Kategori kaydi guncellenemedi");
            return View(model);
        }

        bool updated;
        try
        {
            updated = await _adminCategoryService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var editModel = await _adminCategoryService.GetEditModelAsync(id, cancellationToken);
            model.ParentOptions = editModel?.ParentOptions ?? Array.Empty<CategoryOptionViewModel>();
            SetValidationToast("Kategori kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori guncelleme sirasinda beklenmedik hata. CategoryId: {CategoryId}", id);
            var editModel = await _adminCategoryService.GetEditModelAsync(id, cancellationToken);
            model.ParentOptions = editModel?.ParentOptions ?? Array.Empty<CategoryOptionViewModel>();
            SetUnexpectedErrorToast("Kategori kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Kategori kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    private static CategoryListViewModel ApplyFilters(CategoryListViewModel model, string? q, string? status, string? structure)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var normalizedStructure = string.IsNullOrWhiteSpace(structure) ? "all" : structure.Trim().ToLowerInvariant();

        IEnumerable<CategoryListItemViewModel> query = model.Categories;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.ParentName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedStatus switch
        {
            "active" => query.Where(item => item.IsActive),
            "passive" => query.Where(item => !item.IsActive),
            _ => query
        };

        query = normalizedStructure switch
        {
            "root" => query.Where(item => item.ParentName == "-"),
            "child" => query.Where(item => item.ParentName != "-"),
            _ => query
        };

        var items = query.ToList();

        return new CategoryListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            StructureFilter = normalizedStructure,
            TotalCount = items.Count,
            RootCount = items.Count(item => item.ParentName == "-"),
            ActiveCount = items.Count(item => item.IsActive),
            Categories = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
