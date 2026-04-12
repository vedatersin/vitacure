using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class ProductController : Controller
{
    private readonly IStorefrontContentService _storefrontContentService;

    public ProductController(IStorefrontContentService storefrontContentService)
    {
        _storefrontContentService = storefrontContentService;
    }

    [OutputCache(PolicyName = "StorefrontProduct")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var model = await _storefrontContentService.GetProductDetailPageContentAsync(slug, cancellationToken);
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