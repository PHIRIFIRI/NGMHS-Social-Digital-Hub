using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGMHS.Data;
using NGMHS.Extensions;
using NGMHS.ViewModels;

namespace NGMHS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalForms = await _context.SocialWorkForms.CountAsync();
        ViewBag.TotalExternalQueries = await _context.ExternalQueries.CountAsync();
        ViewBag.TotalOutreachLetters = await _context.OutreachLetters.CountAsync();
        ViewBag.TotalFiles = await _context.FormAttachments.CountAsync() + await _context.OutreachAttachments.CountAsync();

        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminRole(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        // Prevent accidental removal of your own admin access.
        var currentUserId = User.GetUserId();
        if (currentUserId == id)
        {
            TempData["WarningMessage"] = "You cannot change your own role from this screen.";
            return RedirectToAction(nameof(Users));
        }

        user.Role = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? "SocialWorker"
            : "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "User role updated.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Files()
    {
        var formFiles = await _context.FormAttachments
            .Include(a => a.UploadedByUser)
            .Include(a => a.SocialWorkForm)
            .Select(a => new AdminFileViewModel
            {
                Module = "Social Work Form",
                Title = $"Form #{a.SocialWorkFormId} - {a.OriginalFileName}",
                UploadedBy = a.UploadedByUser != null ? a.UploadedByUser.FullName : "Unknown",
                UploadedAtUtc = a.UploadedAtUtc,
                SizeBytes = a.FileSizeBytes,
                DownloadController = "SocialWorkForms",
                DownloadAction = "DownloadAttachment",
                FileId = a.Id
            })
            .ToListAsync();

        var outreachFiles = await _context.OutreachAttachments
            .Include(a => a.OutreachLetter)
            .Select(a => new AdminFileViewModel
            {
                Module = "Outreach Letter",
                Title = $"Outreach #{a.OutreachLetterId} - {a.OriginalFileName}",
                UploadedBy = "N/A",
                UploadedAtUtc = a.UploadedAtUtc,
                SizeBytes = a.FileSizeBytes,
                DownloadController = "Outreach",
                DownloadAction = "DownloadAttachment",
                FileId = a.Id
            })
            .ToListAsync();

        var allFiles = formFiles
            .Concat(outreachFiles)
            .OrderByDescending(f => f.UploadedAtUtc)
            .ToList();

        return View(allFiles);
    }

    public async Task<IActionResult> ExportFormsCsv()
    {
        var rows = await _context.SocialWorkForms
            .Include(f => f.CreatedByUser)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,TemplateCode,TemplateName,ClientFullName,CaseReference,Department,Status,CreatedBy,CreatedAtUtc,UpdatedAtUtc");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",",
                row.Id,
                EscapeCsv(row.TemplateCode),
                EscapeCsv(row.TemplateName),
                EscapeCsv(row.ClientFullName),
                EscapeCsv(row.CaseReference),
                EscapeCsv(row.DepartmentDestination),
                row.Status,
                EscapeCsv(row.CreatedByUser?.FullName ?? "Unknown"),
                row.CreatedAtUtc.ToString("u"),
                row.UpdatedAtUtc.ToString("u")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"NGMHS_Forms_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<IActionResult> ExportQueriesCsv()
    {
        var rows = await _context.ExternalQueries
            .Include(q => q.AssignedToUser)
            .OrderByDescending(q => q.SubmittedAtUtc)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,FullName,Email,Phone,Organisation,Subject,Status,AssignedTo,SubmittedAtUtc,UpdatedAtUtc");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",",
                row.Id,
                EscapeCsv(row.FullName),
                EscapeCsv(row.Email),
                EscapeCsv(row.PhoneNumber),
                EscapeCsv(row.OrganisationName),
                EscapeCsv(row.QuerySubject),
                row.Status,
                EscapeCsv(row.AssignedToUser?.FullName),
                row.SubmittedAtUtc.ToString("u"),
                row.UpdatedAtUtc.ToString("u")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"NGMHS_Queries_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<IActionResult> ExportOutreachCsv()
    {
        var rows = await _context.OutreachLetters
            .Include(o => o.CreatedByUser)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,Type,RecipientName,RecipientEmail,Organisation,Subject,Status,CreatedBy,CreatedAtUtc,UpdatedAtUtc");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",",
                row.Id,
                row.OutreachType,
                EscapeCsv(row.RecipientName),
                EscapeCsv(row.RecipientEmail),
                EscapeCsv(row.RecipientOrganisation),
                EscapeCsv(row.Subject),
                row.Status,
                EscapeCsv(row.CreatedByUser?.FullName),
                row.CreatedAtUtc.ToString("u"),
                row.UpdatedAtUtc.ToString("u")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"NGMHS_Outreach_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
