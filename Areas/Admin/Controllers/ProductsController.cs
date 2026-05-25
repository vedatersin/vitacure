using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using vitacure.Application;
using vitacure.Application.Abstractions;
using vitacure.Domain.Entities;
using vitacure.Domain.Enums;
using vitacure.Infrastructure.Persistence;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class ProductsController : AdminControllerBase
{
    private readonly IAdminProductService _adminProductService;
    private readonly IAdminMediaLibraryService _adminMediaLibraryService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProductsController> _logger;
    private static readonly JsonSerializerOptions FilterJsonOptions = new(JsonSerializerDefaults.Web);

    public ProductsController(IAdminProductService adminProductService, IAdminMediaLibraryService adminMediaLibraryService, AppDbContext dbContext, ILogger<ProductsController> logger)
    {
        _adminProductService = adminProductService;
        _adminMediaLibraryService = adminMediaLibraryService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("/admin/products")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] string? status, [FromQuery] string? stock, CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetProductsAsync(cancellationToken);
        model = ApplyFilters(model, q, status, stock);
        model.SavedFilters = await GetSavedFiltersAsync(cancellationToken);

        if (IsAjaxRequest())
        {
            return PartialView("~/Areas/Admin/Views/Products/_ListContent.cshtml", model);
        }

        return View(model);
    }

    [HttpGet("/admin/products/create")]
    public async Task<IActionResult> Create([FromQuery] string? mode, CancellationToken cancellationToken)
    {
        var model = await _adminProductService.GetCreateModelAsync(mode, cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFeatureSelectionsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            var createModel = await _adminProductService.GetCreateModelAsync(model.CreateMode, cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.GoogleProductCategoryOptions = createModel.GoogleProductCategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.CustomFieldOptions = createModel.CustomFieldOptions;
            model.PersonalizationOptions = createModel.PersonalizationOptions;
            model.TagOptions = createModel.TagOptions;
            model.VariantPresets = createModel.VariantPresets;
            model.BundleProductOptions = createModel.BundleProductOptions;
            SetValidationToast("?r?n kaydi guncellenemedi");
            return View(model);
        }

        try
        {
            await _adminProductService.CreateAsync(model, cancellationToken);
            SetRedirectToast("success", "Kayit basariyla eklendi", "?r?n kaydi olusturuldu.");
        }
        catch (SlugConflictException ex)
        {
            ModelState.AddModelError(nameof(model.Slug), ex.Message);
            var createModel = await _adminProductService.GetCreateModelAsync(model.CreateMode, cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.GoogleProductCategoryOptions = createModel.GoogleProductCategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.CustomFieldOptions = createModel.CustomFieldOptions;
            model.PersonalizationOptions = createModel.PersonalizationOptions;
            model.TagOptions = createModel.TagOptions;
            model.VariantPresets = createModel.VariantPresets;
            model.BundleProductOptions = createModel.BundleProductOptions;
            SetValidationToast("?r?n kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?r?n olusturma sirasinda beklenmedik hata.");
            var createModel = await _adminProductService.GetCreateModelAsync(model.CreateMode, cancellationToken);
            model.BrandOptions = createModel.BrandOptions;
            model.CategoryOptions = createModel.CategoryOptions;
            model.GoogleProductCategoryOptions = createModel.GoogleProductCategoryOptions;
            model.FeatureOptions = createModel.FeatureOptions;
            model.CustomFieldOptions = createModel.CustomFieldOptions;
            model.PersonalizationOptions = createModel.PersonalizationOptions;
            model.TagOptions = createModel.TagOptions;
            model.VariantPresets = createModel.VariantPresets;
            model.BundleProductOptions = createModel.BundleProductOptions;
            SetUnexpectedErrorToast("?r?n kaydi guncellenemedi", ex);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/products/filters")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFilter([FromBody] SaveProductFilterRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var normalizedFilters = NormalizeRules(request.Filters);
        if (normalizedFilters.Count == 0)
        {
            return BadRequest(new { message = "Kaydedilecek en az bir ge�erli filtre se�melisiniz." });
        }

        var maxSortOrder = await _dbContext.ProductSavedFilters
            .Where(x => x.UserId == userId.Value)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var entity = new ProductSavedFilter
        {
            UserId = userId.Value,
            Name = NormalizeFilterName(request.Name),
            FiltersJson = JsonSerializer.Serialize(normalizedFilters, FilterJsonOptions),
            SortOrder = maxSortOrder + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ProductSavedFilters.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new { filter = MapSavedFilter(entity) });
    }

    [HttpPost("/admin/products/filters/{id:int}/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFilter(int id, [FromBody] SaveProductFilterRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindOwnedSavedFilterAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var normalizedFilters = NormalizeRules(request.Filters);
        if (normalizedFilters.Count == 0)
        {
            return BadRequest(new { message = "Kaydedilecek en az bir ge�erli filtre se�melisiniz." });
        }

        entity.Name = NormalizeFilterName(request.Name, entity.Name);
        entity.FiltersJson = JsonSerializer.Serialize(normalizedFilters, FilterJsonOptions);
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new { filter = MapSavedFilter(entity) });
    }

    [HttpPost("/admin/products/filters/{id:int}/rename")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameFilter(int id, [FromBody] RenameProductFilterRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindOwnedSavedFilterAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = NormalizeFilterName(request.Name, entity.Name);
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new { filter = MapSavedFilter(entity) });
    }

    [HttpPost("/admin/products/filters/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFilter(int id, CancellationToken cancellationToken)
    {
        var entity = await FindOwnedSavedFilterAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.ProductSavedFilters.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await NormalizeSavedFilterSortOrderAsync(entity.UserId, cancellationToken);
        return Json(new { success = true });
    }

    [HttpPost("/admin/products/filters/reorder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderFilters([FromBody] ReorderProductFiltersRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var filters = await _dbContext.ProductSavedFilters
            .Where(x => x.UserId == userId.Value)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var incomingIds = (request.FilterIds ?? Array.Empty<int>()).Distinct().ToArray();
        if (incomingIds.Length != filters.Count || incomingIds.Except(filters.Select(x => x.Id)).Any())
        {
            return BadRequest(new { message = "Filtre siralama listesi ge�ersiz." });
        }

        for (var index = 0; index < incomingIds.Length; index += 1)
        {
            var match = filters.First(x => x.Id == incomingIds[index]);
            match.SortOrder = index + 1;
            match.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new
        {
            filters = filters
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(MapSavedFilter)
                .ToArray()
        });
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
            model.GoogleProductCategoryOptions = editModel?.GoogleProductCategoryOptions ?? Array.Empty<ProductGoogleCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.CustomFieldOptions = editModel?.CustomFieldOptions ?? Array.Empty<ProductCustomFieldOptionViewModel>();
            model.PersonalizationOptions = editModel?.PersonalizationOptions ?? Array.Empty<ProductPersonalizationOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            model.VariantPresets = editModel?.VariantPresets ?? Array.Empty<ProductVariantPresetViewModel>();
            model.BundleProductOptions = editModel?.BundleProductOptions ?? Array.Empty<ProductBundleProductOptionViewModel>();
            SetValidationToast("?r?n kaydi guncellenemedi");
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
            model.GoogleProductCategoryOptions = editModel?.GoogleProductCategoryOptions ?? Array.Empty<ProductGoogleCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.CustomFieldOptions = editModel?.CustomFieldOptions ?? Array.Empty<ProductCustomFieldOptionViewModel>();
            model.PersonalizationOptions = editModel?.PersonalizationOptions ?? Array.Empty<ProductPersonalizationOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            model.VariantPresets = editModel?.VariantPresets ?? Array.Empty<ProductVariantPresetViewModel>();
            model.BundleProductOptions = editModel?.BundleProductOptions ?? Array.Empty<ProductBundleProductOptionViewModel>();
            SetValidationToast("?r?n kaydi guncellenemedi");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?r?n guncelleme sirasinda beklenmedik hata. ProductId: {ProductId}", id);
            var editModel = await _adminProductService.GetEditModelAsync(id, cancellationToken);
            model.BrandOptions = editModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
            model.CategoryOptions = editModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
            model.GoogleProductCategoryOptions = editModel?.GoogleProductCategoryOptions ?? Array.Empty<ProductGoogleCategoryOptionViewModel>();
            model.FeatureOptions = editModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
            model.CustomFieldOptions = editModel?.CustomFieldOptions ?? Array.Empty<ProductCustomFieldOptionViewModel>();
            model.PersonalizationOptions = editModel?.PersonalizationOptions ?? Array.Empty<ProductPersonalizationOptionViewModel>();
            model.TagOptions = editModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
            model.VariantPresets = editModel?.VariantPresets ?? Array.Empty<ProductVariantPresetViewModel>();
            model.BundleProductOptions = editModel?.BundleProductOptions ?? Array.Empty<ProductBundleProductOptionViewModel>();
            SetUnexpectedErrorToast("?r?n kaydi guncellenemedi", ex);
            return View(model);
        }

        if (!updated)
        {
            return NotFound();
        }

        SetRedirectToast("success", "Kayit basariyla guncellendi", "?r?n kaydi guncellendi.");
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
            _logger.LogError(ex, "?r?n gorseli yuklenirken beklenmedik hata.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "G?rsel yuklenemedi." });
        }
    }

    [HttpPost("/admin/products/custom-fields/quick-create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreateCustomField([FromBody] QuickCreateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { ["Name"] = new[] { "?zel alan adi zorunludur." } } });
        }

        var slug = Slugify(string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug!);
        if (await _dbContext.CustomFieldDefinitions.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 9, 150)];
        }

        var entity = new CustomFieldDefinition
        {
            Name = name,
            Slug = slug,
            FieldType = NormalizeDefinitionType(request.FieldType, "HTML"),
            IsFilterable = request.IsFilterable,
            IsActive = true
        };

        _dbContext.CustomFieldDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new
        {
            customField = new
            {
                id = entity.Id,
                name = entity.Name,
                slug = entity.Slug,
                fieldType = entity.FieldType,
                isFilterable = entity.IsFilterable
            }
        });
    }

    [HttpPost("/admin/products/personalizations/quick-create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreatePersonalization([FromBody] QuickCreatePersonalizationRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { ["Name"] = new[] { "?zellestirme adi zorunludur." } } });
        }

        var slug = Slugify(string.IsNullOrWhiteSpace(request.Slug) ? name : request.Slug!);
        if (await _dbContext.PersonalizationDefinitions.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 9, 150)];
        }

        var entity = new PersonalizationDefinition
        {
            Name = name,
            Slug = slug,
            InputType = NormalizeDefinitionType(request.InputType, "Text"),
            IsActive = true
        };

        _dbContext.PersonalizationDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Json(new
        {
            personalization = new
            {
                id = entity.Id,
                name = entity.Name,
                slug = entity.Slug,
                inputType = entity.InputType
            }
        });
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
            "publishedopen" => query.Where(item => item.Status == ProductPublishingStatus.PublishedOpen),
            "publishedclosed" => query.Where(item => item.Status == ProductPublishingStatus.PublishedClosed),
            "archived" => query.Where(item => item.Status == ProductPublishingStatus.Archived),
            "draft" => query.Where(item => item.Status == ProductPublishingStatus.Draft),
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
            BrandOptions = model.BrandOptions,
            CategoryOptions = model.CategoryOptions,
            TagOptions = model.TagOptions,
            SalesChannelOptions = model.SalesChannelOptions,
            SavedFilters = model.SavedFilters,
            Products = items
        };
    }

    private int? GetCurrentUserId()
    {
        var rawValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(rawValue, out var userId) ? userId : null;
    }

    private async Task<IReadOnlyList<ProductSavedFilterViewModel>> GetSavedFiltersAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Array.Empty<ProductSavedFilterViewModel>();
        }

        var filters = await _dbContext.ProductSavedFilters
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return filters.Select(MapSavedFilter).ToArray();
    }

    private async Task<ProductSavedFilter?> FindOwnedSavedFilterAsync(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return null;
        }

        return await _dbContext.ProductSavedFilters
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId.Value, cancellationToken);
    }

    private async Task NormalizeSavedFilterSortOrderAsync(int userId, CancellationToken cancellationToken)
    {
        var filters = await _dbContext.ProductSavedFilters
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        for (var index = 0; index < filters.Count; index += 1)
        {
            filters[index].SortOrder = index + 1;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProductSavedFilterViewModel MapSavedFilter(ProductSavedFilter entity)
    {
        return new ProductSavedFilterViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            SortOrder = entity.SortOrder,
            Filters = DeserializeRules(entity.FiltersJson)
        };
    }

    private static IReadOnlyList<ProductFilterRuleViewModel> DeserializeRules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<ProductFilterRuleViewModel>();
        }

        try
        {
            var rules = JsonSerializer.Deserialize<List<ProductFilterRuleViewModel>>(json, FilterJsonOptions);
            return NormalizeRules(rules).Select(MapRule).ToArray();
        }
        catch
        {
            return Array.Empty<ProductFilterRuleViewModel>();
        }
    }

    private static IReadOnlyList<ProductFilterRulePayload> NormalizeRules(IEnumerable<ProductFilterRuleViewModel>? rules)
    {
        return (rules ?? Array.Empty<ProductFilterRuleViewModel>())
            .Select(rule => new ProductFilterRulePayload
            {
                Field = (rule.Field ?? string.Empty).Trim(),
                Operator = (rule.Operator ?? string.Empty).Trim(),
                Value = string.IsNullOrWhiteSpace(rule.Value) ? null : rule.Value.Trim(),
                Values = (rule.Values ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            })
            .Where(rule => !string.IsNullOrWhiteSpace(rule.Field) &&
                           !string.IsNullOrWhiteSpace(rule.Operator) &&
                           (rule.Values.Count > 0 || !string.IsNullOrWhiteSpace(rule.Value)))
            .ToArray();
    }

    private static ProductFilterRuleViewModel MapRule(ProductFilterRulePayload rule)
    {
        return new ProductFilterRuleViewModel
        {
            Field = rule.Field,
            Operator = rule.Operator,
            Value = rule.Value,
            Values = rule.Values
        };
    }

    private static string NormalizeFilterName(string? name, string fallback = "Isimsiz Filtre")
    {
        return string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private async Task ValidateFeatureSelectionsAsync(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        var sourceModel = model.Id.HasValue
            ? await _adminProductService.GetEditModelAsync(model.Id.Value, cancellationToken)
            : await _adminProductService.GetCreateModelAsync(model.CreateMode, cancellationToken);

        model.BrandOptions = sourceModel?.BrandOptions ?? Array.Empty<ProductBrandOptionViewModel>();
        model.CategoryOptions = sourceModel?.CategoryOptions ?? Array.Empty<ProductCategoryOptionViewModel>();
        model.GoogleProductCategoryOptions = sourceModel?.GoogleProductCategoryOptions ?? Array.Empty<ProductGoogleCategoryOptionViewModel>();
        model.FeatureOptions = sourceModel?.FeatureOptions ?? Array.Empty<ProductFeatureOptionViewModel>();
        model.CustomFieldOptions = sourceModel?.CustomFieldOptions ?? Array.Empty<ProductCustomFieldOptionViewModel>();
        model.PersonalizationOptions = sourceModel?.PersonalizationOptions ?? Array.Empty<ProductPersonalizationOptionViewModel>();
        model.TagOptions = sourceModel?.TagOptions ?? Array.Empty<ProductTagOptionViewModel>();
        model.BundleProductOptions = sourceModel?.BundleProductOptions ?? Array.Empty<ProductBundleProductOptionViewModel>();
        model.SelectedCategoryIds = model.SelectedCategoryIds
            .Where(x => x > 0)
            .Concat(model.CategoryId is > 0 ? new[] { model.CategoryId.Value } : Array.Empty<int>())
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

    private static string NormalizeDefinitionType(string? value, string fallback)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static string Slugify(string value)
    {
        return value.Trim().ToLowerInvariant()
            .Replace("�", "c")
            .Replace("g", "g")
            .Replace("i", "i")
            .Replace("�", "o")
            .Replace("s", "s")
            .Replace("�", "u")
            .Replace("&", string.Empty)
            .Replace("+", "plus")
            .Replace("  ", " ")
            .Replace(" ", "-");
    }

    public class SaveProductFilterRequest
    {
        public string? Name { get; set; }
        public IReadOnlyList<ProductFilterRuleViewModel>? Filters { get; set; }
    }

    public class RenameProductFilterRequest
    {
        public string? Name { get; set; }
    }

    public class ReorderProductFiltersRequest
    {
        public IReadOnlyList<int>? FilterIds { get; set; }
    }

    public class QuickCreateCustomFieldRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? FieldType { get; set; }
        public bool IsFilterable { get; set; }
    }

    public class QuickCreatePersonalizationRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? InputType { get; set; }
    }

    private sealed class ProductFilterRulePayload
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string? Value { get; set; }
        public IReadOnlyList<string> Values { get; set; } = Array.Empty<string>();
    }
}
