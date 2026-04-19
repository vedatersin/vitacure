using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class AiController : Controller
{
    [HttpGet("/admin/ai")]
    public IActionResult Index()
    {
        ViewData["Title"] = "AI";
        return View();
    }
}
