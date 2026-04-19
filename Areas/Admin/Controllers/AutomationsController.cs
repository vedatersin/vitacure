using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class AutomationsController : Controller
{
    [HttpGet("/admin/automations")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Otomasyonlar";
        return View();
    }
}
