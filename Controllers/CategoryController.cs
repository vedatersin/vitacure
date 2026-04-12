using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class CategoryController : Controller
{
    private readonly IStorefrontContentService _storefrontContentService;

    public CategoryController(IStorefrontContentService storefrontContentService)
    {
        _storefrontContentService = storefrontContentService;
    }

    [OutputCache(PolicyName = "StorefrontCategory")]
    public async Task<IActionResult> Detail(string slug, string? tag, CancellationToken cancellationToken)
    {
        var model = await _storefrontContentService.GetCategoryPageContentAsync(slug, tag, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        ViewData["Title"] = model.Title;
        ViewData["MetaDescription"] = model.MetaDescription;
        ViewData["CanonicalPath"] = model.CanonicalPath;

        return View(model);
    }
}