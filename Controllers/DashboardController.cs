using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGMHS.Data;
using NGMHS.Extensions;
using NGMHS.Models;
using NGMHS.ViewModels;

namespace NGMHS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = User.IsAdmin();
        var formQuery = _context.SocialWorkForms.AsQueryable();
        var outreachQuery = _context.OutreachLetters.AsQueryable();

        if (!isAdmin)
        {
            formQuery = formQuery.Where(f => f.CreatedByUserId == userId.Value);
            outreachQuery = outreachQuery.Where(o => o.CreatedByUserId == userId.Value);
        }

        var model = new DashboardViewModel
        {
            FullName = User.GetDisplayName(),
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "SocialWorker",
            ActiveForms = await formQuery.CountAsync(f => f.Status == WorkFormStatus.Active),
            CompletedForms = await formQuery.CountAsync(f => f.Status == WorkFormStatus.Completed),
            SubmittedForms = await formQuery.CountAsync(f => f.Status == WorkFormStatus.Submitted),
            OpenExternalQueries = await _context.ExternalQueries.CountAsync(q =>
                q.Status == ExternalQueryStatus.New || q.Status == ExternalQueryStatus.InReview),
            DraftOutreachLetters = await outreachQuery.CountAsync(o =>
                o.Status == OutreachStatus.Draft || o.Status == OutreachStatus.ReadyToSend)
        };

        return View(model);
    }
}
