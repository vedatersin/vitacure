using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class SlugController : Controller
{
    private readonly ISlugService _slugService;
    private readonly IStorefrontContentService _storefrontContentService;

    public SlugController(ISlugService slugService, IStorefrontContentService storefrontContentService)
    {
        _slugService = slugService;
        _storefrontContentService = storefrontContentService;
    }

    public async Task<IActionResult> Resolve(string slug, string? tag, CancellationToken cancellationToken)
    {
        var match = await _slugService.ResolveStorefrontAsync(slug, cancellationToken);

        if (match.TargetType == StorefrontSlugTargetType.Product)
        {
            var model = await _storefrontContentService.GetProductDetailPageContentAsync(match.Slug, cancellationToken);
            if (model is null)
            {
                return NotFound();
            }

            ViewData["Title"] = model.Title;
            ViewData["MetaDescription"] = model.MetaDescription;
            ViewData["CanonicalPath"] = model.CanonicalPath;

            return View("~/Views/Product/Detail.cshtml", model);
        }

        if (match.TargetType == StorefrontSlugTargetType.Showcase)
        {
            var model = await _storefrontContentService.GetShowcasePageContentAsync(match.Slug, tag, cancellationToken);
            if (model is null)
            {
                return NotFound();
            }

            ViewData["Title"] = model.Title;
            ViewData["MetaDescription"] = model.MetaDescription;
            ViewData["CanonicalPath"] = model.CanonicalPath;

            return View("~/Views/Showcase/Detail.cshtml", model);
        }

        if (match.TargetType == StorefrontSlugTargetType.Category)
        {
            var model = await _storefrontContentService.GetCategoryPageContentAsync(match.Slug, tag, cancellationToken);
            if (model is null)
            {
                return NotFound();
            }

            ViewData["Title"] = model.Title;
            ViewData["MetaDescription"] = model.MetaDescription;
            ViewData["CanonicalPath"] = model.CanonicalPath;

            return View("~/Views/Category/Detail.cshtml", model);
        }

        return NotFound();
    }
}
