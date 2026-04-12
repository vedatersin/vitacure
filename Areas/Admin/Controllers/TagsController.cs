using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vitacure.Application.Abstractions;
using vitacure.Models.ViewModels.Admin;

namespace vitacure.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class TagsController : Controller
{
    private readonly IAdminTagService _adminTagService;

    public TagsController(IAdminTagService adminTagService)
    {
        _adminTagService = adminTagService;
    }

    [HttpGet("/admin/tags")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetTagsAsync(cancellationToken);
        return View(model);
    }

    [HttpGet("/admin/tags/create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("/admin/tags/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _adminTagService.CreateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/tags/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _adminTagService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost("/admin/tags/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TagFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _adminTagService.UpdateAsync(model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
