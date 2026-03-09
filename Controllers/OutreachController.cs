using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGMHS.Data;
using NGMHS.Extensions;
using NGMHS.Models;
using NGMHS.Services;
using NGMHS.ViewModels;

namespace NGMHS.Controllers;

[Authorize]
public class OutreachController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public OutreachController(ApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index(string? statusFilter, string? typeFilter)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var query = _context.OutreachLetters
            .Include(o => o.CreatedByUser)
            .AsQueryable();

        if (!User.IsAdmin())
        {
            query = query.Where(o => o.CreatedByUserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            Enum.TryParse<OutreachStatus>(statusFilter, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) &&
            Enum.TryParse<OutreachType>(typeFilter, true, out var type))
        {
            query = query.Where(o => o.OutreachType == type);
        }

        ViewData["StatusFilter"] = statusFilter;
        ViewData["TypeFilter"] = typeFilter;

        var letters = await query
            .OrderByDescending(o => o.UpdatedAtUtc)
            .ToListAsync();

        return View(letters);
    }

    public IActionResult Create()
    {
        var model = new OutreachInputViewModel
        {
            Status = OutreachStatus.Draft
        };

        PopulateOptions(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OutreachInputViewModel model, List<IFormFile>? files)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        PopulateOptions(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var letter = new OutreachLetter
        {
            OutreachType = model.OutreachType,
            RecipientName = model.RecipientName.Trim(),
            RecipientEmail = model.RecipientEmail?.Trim(),
            RecipientOrganisation = model.RecipientOrganisation?.Trim(),
            Subject = model.Subject.Trim(),
            LetterBody = model.LetterBody,
            Status = model.Status,
            CreatedByUserId = userId.Value,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.OutreachLetters.Add(letter);
        await _context.SaveChangesAsync();

        var warnings = await SaveAttachmentsAsync(letter.Id, files);
        if (warnings.Count > 0)
        {
            TempData["WarningMessage"] = string.Join(" ", warnings);
        }

        TempData["SuccessMessage"] = "Outreach letter created.";
        return RedirectToAction(nameof(Details), new { id = letter.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        return View(letter);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        var model = new OutreachInputViewModel
        {
            Id = letter.Id,
            OutreachType = letter.OutreachType,
            RecipientName = letter.RecipientName,
            RecipientEmail = letter.RecipientEmail,
            RecipientOrganisation = letter.RecipientOrganisation,
            Subject = letter.Subject,
            LetterBody = letter.LetterBody,
            Status = letter.Status
        };

        PopulateOptions(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OutreachInputViewModel model, List<IFormFile>? files)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        PopulateOptions(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        letter.OutreachType = model.OutreachType;
        letter.RecipientName = model.RecipientName.Trim();
        letter.RecipientEmail = model.RecipientEmail?.Trim();
        letter.RecipientOrganisation = model.RecipientOrganisation?.Trim();
        letter.Subject = model.Subject.Trim();
        letter.LetterBody = model.LetterBody;
        letter.Status = model.Status;
        letter.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var warnings = await SaveAttachmentsAsync(letter.Id, files);
        if (warnings.Count > 0)
        {
            TempData["WarningMessage"] = string.Join(" ", warnings);
        }

        TempData["SuccessMessage"] = "Outreach letter updated.";
        return RedirectToAction(nameof(Details), new { id = letter.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        return View(letter);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        foreach (var attachment in letter.Attachments)
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoragePath);
        }

        _context.OutreachLetters.Remove(letter);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Outreach letter deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Print(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        return View(letter);
    }

    public async Task<IActionResult> DownloadSummary(int id)
    {
        var letter = await LoadAuthorizedLetterAsync(id);
        if (letter is null)
        {
            return NotFound();
        }

        var content = BuildSummary(letter);
        var bytes = Encoding.UTF8.GetBytes(content);
        var filename = $"OutreachLetter_{letter.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

        return File(bytes, "text/plain", filename);
    }

    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var attachment = await _context.OutreachAttachments
            .Include(a => a.OutreachLetter)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment is null || attachment.OutreachLetter is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!User.IsAdmin() && attachment.OutreachLetter.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        var stream = await _fileStorageService.OpenReadAsync(attachment.StoragePath);
        return File(stream, attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int id, int outreachId)
    {
        var attachment = await _context.OutreachAttachments
            .Include(a => a.OutreachLetter)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment is null || attachment.OutreachLetter is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!User.IsAdmin() && attachment.OutreachLetter.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        await _fileStorageService.DeleteFileAsync(attachment.StoragePath);

        _context.OutreachAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Attachment deleted.";
        return RedirectToAction(nameof(Details), new { id = outreachId });
    }

    private async Task<OutreachLetter?> LoadAuthorizedLetterAsync(int id)
    {
        var letter = await _context.OutreachLetters
            .Include(o => o.CreatedByUser)
            .Include(o => o.Attachments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (letter is null)
        {
            return null;
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return null;
        }

        if (!User.IsAdmin() && letter.CreatedByUserId != userId.Value)
        {
            return null;
        }

        return letter;
    }

    private async Task<List<string>> SaveAttachmentsAsync(int outreachLetterId, List<IFormFile>? files)
    {
        var warnings = new List<string>();

        if (files is null || files.Count == 0)
        {
            return warnings;
        }

        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                var stored = await _fileStorageService.SaveFileAsync(file, "outreach");
                _context.OutreachAttachments.Add(new OutreachAttachment
                {
                    OutreachLetterId = outreachLetterId,
                    OriginalFileName = stored.OriginalFileName,
                    StoragePath = stored.StoragePath,
                    ContentType = stored.ContentType,
                    FileSizeBytes = stored.FileSizeBytes,
                    UploadedAtUtc = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                warnings.Add($"{file.FileName}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        return warnings;
    }

    private static string BuildSummary(OutreachLetter letter)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"NGMHS OUTREACH LETTER #{letter.Id}");
        sb.AppendLine($"Type: {letter.OutreachType}");
        sb.AppendLine($"Recipient: {letter.RecipientName}");
        sb.AppendLine($"Recipient Email: {letter.RecipientEmail ?? "N/A"}");
        sb.AppendLine($"Recipient Organisation: {letter.RecipientOrganisation ?? "N/A"}");
        sb.AppendLine($"Subject: {letter.Subject}");
        sb.AppendLine($"Status: {letter.Status}");
        sb.AppendLine($"Created (UTC): {letter.CreatedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Updated (UTC): {letter.UpdatedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        sb.AppendLine("LETTER BODY");
        sb.AppendLine("-----------");
        sb.AppendLine(letter.LetterBody);

        return sb.ToString();
    }

    private static void PopulateOptions(OutreachInputViewModel model)
    {
        model.OutreachTypeOptions = Enum.GetValues<OutreachType>()
            .Select(type => new SelectListItem
            {
                Value = type.ToString(),
                Text = type.ToString(),
                Selected = type == model.OutreachType
            })
            .ToList();
    }
}

