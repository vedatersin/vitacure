using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class CategoriesController : Controller
{
    private readonly IAdminCategoryService _adminCategoryService;

    public CategoriesController(IAdminCategoryService adminCategoryService)
    {
        _adminCategoryService = adminCategoryService;
    }

    [HttpGet("/admin/categories")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminCategoryService.GetCategoriesAsync(cancellationToken);
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
            return View(model);
        }

        await _adminCategoryService.CreateAsync(model, cancellationToken);
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
            return View(model);
        }

        var updated = await _adminCategoryService.UpdateAsync(model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
