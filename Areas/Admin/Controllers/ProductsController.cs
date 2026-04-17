using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application;
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
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? stock, CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetProductsAsync(cancellationToken);
        model = ApplyFilters(model, q, status, stock);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Products/_ListContent.cshtml", model);
        }

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

        try
        {
            await _adminProductService.CreateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.CategoryOptions = createModel.CategoryOptions;
            model.TagOptions = createModel.TagOptions;
            return View(model);
        }

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

        bool updated;
        try
        {
            updated = await _adminProductService.UpdateAsync(model, cancellationToken);
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private static ProductListViewModel ApplyFilters(ProductListViewModel model, string? q, string? status, string? stock)
    {
        var search = q?.Trim();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var normalizedStock = string.IsNullOrWhiteSpace(stock) ? "all" : stock.Trim().ToLowerInvariant();

        IEnumerable<ProductListItemViewModel> query = model.Products;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = normalizedStatus switch
        {
            "active" => query.Where(item => item.IsActive),
            "passive" => query.Where(item => !item.IsActive),
            _ => query
        };

        query = normalizedStock switch
        {
            "instock" => query.Where(item => item.Stock > 0),
            "outofstock" => query.Where(item => item.Stock <= 0),
            _ => query
        };

        var items = query.ToList();

        return new ProductListViewModel
        {
            SearchTerm = search,
            StatusFilter = normalizedStatus,
            StockFilter = normalizedStock,
            TotalCount = items.Count,
            ActiveCount = items.Count(item => item.IsActive),
            OutOfStockCount = items.Count(item => item.Stock <= 0),
            Products = items
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
