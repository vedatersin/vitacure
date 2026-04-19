using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class ProductsController : AdminControllerBase
{
    private readonly IAdminProductService _adminProductService;
    private readonly IAdminMediaLibraryService _adminMediaLibraryService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IAdminProductService adminProductService, IAdminMediaLibraryService adminMediaLibraryService, ILogger<ProductsController> logger)
    {
        _adminProductService = adminProductService;
        _adminMediaLibraryService = adminMediaLibraryService;
        _logger = logger;
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
        await ValidateFeatureSelectionsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.TagOptions = createModel.TagOptions;
            SetValidationToast("Urun kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminProductService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "Urun kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.TagOptions = createModel.TagOptions;
            SetValidationToast("Urun kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun olusturma sirasinda beklenmedik hata.");
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.TagOptions = createModel.TagOptions;
            SetUnexpectedErrorToast("Urun kaydi guncellenemedi", ex);
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

        await ValidateFeatureSelectionsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.BrandOptions = editModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            SetValidationToast("Urun kaydi guncellenemedi");
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
            model.BrandOptions = editModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            SetValidationToast("Urun kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun guncelleme sirasinda beklenmedik hata. ProductId: {ProductId}", id);
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.BrandOptions = editModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            SetUnexpectedErrorToast("Urun kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "Urun kaydi guncellendi.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/products/upload-image")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string? slug, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _adminMediaLibraryService.UploadAsync(file, slug, cancellationToken);
            return Json(new { url = item.Url, id = item.Id, name = item.OriginalFileName });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun gorseli yuklenirken beklenmedik hata.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Gorsel yuklenemedi." });
        }
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
                item.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.BrandName.Contains(search, StringComparison.OrdinalIgnoreCase));
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

    private async Task ValidateFeatureSelectionsAsync(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        var sourceModel = model.Id.HasValue
            ? await _adminProductService.GetEditModelAsync(model.Id.Value, cancellationToken)
            : await _adminProductService.GetCreateModelAsync(cancellationToken);

        model.BrandOptions = sourceModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
        model.CategoryOptions = sourceModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
        model.FeatureOptions = sourceModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
        model.TagOptions = sourceModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
        model.SelectedCategoryIds = model.SelectedCategoryIds
            .Where(x => x > 0)
            .Append(model.CategoryId)
            .Distinct()
            .ToArray();

        var selectedFeatureIds = model.SelectedFeatureIds
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet();

        foreach (var feature in model.FeatureOptions.Where(feature => selectedFeatureIds.Contains(feature.Id) && feature.Options.Count > 0))
        {
            var hasValue = model.SelectedFeatureValues.TryGetValue(feature.Id, out var value) &&
                           !string.IsNullOrWhiteSpace(value);

            if (!hasValue)
            {
                ModelState.AddModelError(nameof(model.SelectedFeatureIds), $"'{feature.Name}' icin bir secenek belirlemelisiniz.");
            }
        }

        var variants = model.Variants
            .Where(variant => !string.IsNullOrWhiteSpace(variant.GroupName) || !string.IsNullOrWhiteSpace(variant.OptionName) || !string.IsNullOrWhiteSpace(variant.Sku))
            .ToList();

        for (var index = 0; index < variants.Count; index++)
        {
            var variant = variants[index];
            if (string.IsNullOrWhiteSpace(variant.GroupName) || string.IsNullOrWhiteSpace(variant.OptionName))
            {
                ModelState.AddModelError(nameof(model.Variants), $"Variant satiri {index + 1} icin eksen ve secenek alanlari zorunludur.");
            }

            if (variant.Price <= 0)
            {
                ModelState.AddModelError(nameof(model.Variants), $"Variant satiri {index + 1} icin fiyat sifirdan buyuk olmalidir.");
            }
        }
    }
}
