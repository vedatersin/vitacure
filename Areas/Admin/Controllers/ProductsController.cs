using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class ProductsController : Controller
{
    private readonly IAdminProductService _adminProductService;

    public ProductsController(IAdminProductService adminProductService)
    {
        _adminProductService = adminProductService;
    }

    [HttpGet("/admin/products")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetProductsAsync(cancellationToken);
        return View(model);
    }

    [HttpGet("/admin/products/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.CategoryOptions = createModel.CategoryOptions;
            model.TagOptions = createModel.TagOptions;
            return View(model);
        }

        await _adminProductService.CreateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/products/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/products/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            return View(model);
        }

        var updated = await _adminProductService.UpdateAsync(model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
