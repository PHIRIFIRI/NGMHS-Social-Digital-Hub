using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGMHS.Data;
using NGMHS.Models;
using NGMHS.ViewModels;

namespace NGMHS.Controllers;

public class ExternalQueryController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExternalQueryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Public form where external users can submit support requests.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ExternalQueryCreateViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExternalQueryCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var query = new ExternalQuery
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            OrganisationName = model.OrganisationName?.Trim(),
            QuerySubject = model.QuerySubject.Trim(),
            Message = model.Message,
            Status = ExternalQueryStatus.New,
            SubmittedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.ExternalQueries.Add(query);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ThankYou));
    }

    [AllowAnonymous]
    public IActionResult ThankYou()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Index(string? statusFilter)
    {
        var query = _context.ExternalQueries
            .Include(x => x.AssignedToUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            Enum.TryParse<ExternalQueryStatus>(statusFilter, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        ViewData["StatusFilter"] = statusFilter;

        var list = await query
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ToListAsync();

        return View(list);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.ExternalQueries
            .Include(q => q.AssignedToUser)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        await PopulateSocialWorkerListAsync(item.AssignedToUserId);

        var model = new ExternalQueryManageViewModel
        {
            Id = item.Id,
            FullName = item.FullName,
            Email = item.Email,
            PhoneNumber = item.PhoneNumber,
            OrganisationName = item.OrganisationName,
            QuerySubject = item.QuerySubject,
            Message = item.Message,
            Status = item.Status,
            ResponseNotes = item.ResponseNotes,
            AssignedToUserId = item.AssignedToUserId
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(int id, ExternalQueryManageViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var item = await _context.ExternalQueries.FirstOrDefaultAsync(q => q.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateSocialWorkerListAsync(model.AssignedToUserId);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        item.Status = model.Status;
        item.ResponseNotes = model.ResponseNotes?.Trim();
        item.AssignedToUserId = model.AssignedToUserId;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Query updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ExternalQueries.FirstOrDefaultAsync(q => q.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        _context.ExternalQueries.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "External query deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSocialWorkerListAsync(int? selectedValue)
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        ViewBag.AssignedUsers = users.Select(user => new SelectListItem
        {
            Value = user.Id.ToString(),
            Text = $"{user.FullName} ({user.Role})",
            Selected = selectedValue.HasValue && selectedValue.Value == user.Id
        }).ToList();
    }
}
