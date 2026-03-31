using Microsoft.AspNetCore.Mvc;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class CategoryController : Controller
{
    private readonly IMockContentService _mockContentService;

    public CategoryController(IMockContentService mockContentService)
    {
        _mockContentService = mockContentService;
    }

    public IActionResult Detail(string slug)
    {
        var model = _mockContentService.GetCategoryPageContent(slug);
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
