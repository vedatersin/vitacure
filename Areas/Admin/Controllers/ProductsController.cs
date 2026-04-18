using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    };

    private readonly IAdminProductService _adminProductService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IAdminProductService adminProductService, IWebHostEnvironment environment, ILogger<ProductsController> logger)
    {
        _adminProductService = adminProductService;
        _environment = environment;
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
        if (!ModelState.IsValid)
        {
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.CategoryOptions = createModel.CategoryOptions;
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
            model.CategoryOptions = createModel.CategoryOptions;
            model.TagOptions = createModel.TagOptions;
            SetValidationToast("Urun kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun olusturma sirasinda beklenmedik hata.");
            var createModel = await _adminProductService.GetCreateModelAsync(cancellationToken);
            model.CategoryOptions = createModel.CategoryOptions;
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

        if (!ModelState.IsValid)
        {
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
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
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            SetValidationToast("Urun kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun guncelleme sirasinda beklenmedik hata. ProductId: {ProductId}", id);
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
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
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Gorsel secilmedi." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            return BadRequest(new { error = "Desteklenmeyen gorsel formati." });
        }

        var uploadsDirectory = Path.Combine(_environment.WebRootPath, "img", "products");
        Directory.CreateDirectory(uploadsDirectory);

        var safeSlug = NormalizeFileSegment(slug);
        var fileName = $"{safeSlug}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return Json(new { url = $"/img/products/{fileName}" });
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

    private static string NormalizeFileSegment(string? value)
    {
        var raw = string.IsNullOrWhiteSpace(value) ? "product" : value.Trim().ToLowerInvariant();
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(raw.Select(character => invalidChars.Contains(character) ? '-' : character).ToArray());

        return sanitized
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}
