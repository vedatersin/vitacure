using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using vitacure.Models;
using vitacure.Services.Content;

namespace vitacure.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMockContentService _mockContentService;

    public HomeController(ILogger<HomeController> logger, IMockContentService mockContentService)
    {
        _logger = logger;
        _mockContentService = mockContentService;
    }

    public IActionResult Index()
    {
        var model = _mockContentService.GetHomePageContent();
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
