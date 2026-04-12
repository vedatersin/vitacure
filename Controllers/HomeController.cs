using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using vitacure.Models;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class HomeController : Controller
{
    private readonly IStorefrontContentService _storefrontContentService;

    public HomeController(IStorefrontContentService storefrontContentService)
    {
        _storefrontContentService = storefrontContentService;
    }

    [OutputCache(PolicyName = "StorefrontHome")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _storefrontContentService.GetHomePageContentAsync(cancellationToken);
        ViewData["Title"] = model.Title;
        ViewData["MetaDescription"] = model.MetaDescription;
        ViewData["CanonicalPath"] = model.CanonicalPath;

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}