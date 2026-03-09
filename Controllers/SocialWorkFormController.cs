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
public class SocialWorkFormsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFormTemplateService _templateService;
    private readonly IFileStorageService _fileStorageService;

    public SocialWorkFormsController(
        ApplicationDbContext context,
        IFormTemplateService templateService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _templateService = templateService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index(string? statusFilter, string? search)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = User.IsAdmin();
        var query = _context.SocialWorkForms
            .Include(f => f.CreatedByUser)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(f => f.CreatedByUserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            Enum.TryParse<WorkFormStatus>(statusFilter, true, out var parsedStatus))
        {
            query = query.Where(f => f.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(f =>
                f.ClientFullName.ToLower().Contains(term) ||
                f.CaseReference.ToLower().Contains(term) ||
                f.TemplateName.ToLower().Contains(term));
        }

        ViewData["StatusFilter"] = statusFilter;
        ViewData["Search"] = search;

        var forms = await query
            .OrderByDescending(f => f.UpdatedAtUtc)
            .ToListAsync();

        return View(forms);
    }

    public IActionResult Create()
    {
        var model = new SocialWorkFormInputViewModel
        {
            Status = WorkFormStatus.Active,
            DepartmentDestination = "DSD"
        };

        PopulateLookups(model);

        if (string.IsNullOrWhiteSpace(model.FormBody) && model.TemplateBodies.TryGetValue(model.TemplateCode, out var body))
        {
            model.FormBody = body;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SocialWorkFormInputViewModel model, List<IFormFile>? files)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        PopulateLookups(model);

        var template = _templateService.GetTemplateByCode(model.TemplateCode);
        if (template is null)
        {
            ModelState.AddModelError(nameof(model.TemplateCode), "Please select a valid form template.");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.FormBody))
        {
            model.FormBody = template.DefaultBody;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var form = new SocialWorkForm
        {
            TemplateCode = template.Code,
            TemplateName = template.Name,
            ClientFullName = model.ClientFullName.Trim(),
            ClientReferenceNumber = model.ClientReferenceNumber?.Trim(),
            CaseReference = model.CaseReference.Trim(),
            DepartmentDestination = model.DepartmentDestination.Trim(),
            FormBody = model.FormBody,
            InternalNotes = model.InternalNotes,
            Status = model.Status,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId.Value
        };

        _context.SocialWorkForms.Add(form);
        await _context.SaveChangesAsync();

        var warnings = await SaveFormAttachmentsAsync(form.Id, userId.Value, files);

        if (warnings.Count > 0)
        {
            TempData["WarningMessage"] = string.Join(" ", warnings);
        }

        TempData["SuccessMessage"] = "Form created successfully.";
        return RedirectToAction(nameof(Details), new { id = form.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        return View(form);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        var model = new SocialWorkFormInputViewModel
        {
            Id = form.Id,
            TemplateCode = form.TemplateCode,
            ClientFullName = form.ClientFullName,
            ClientReferenceNumber = form.ClientReferenceNumber,
            CaseReference = form.CaseReference,
            DepartmentDestination = form.DepartmentDestination,
            FormBody = form.FormBody,
            InternalNotes = form.InternalNotes,
            Status = form.Status
        };

        PopulateLookups(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SocialWorkFormInputViewModel model, List<IFormFile>? files)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        PopulateLookups(model);

        var template = _templateService.GetTemplateByCode(model.TemplateCode);
        if (template is null)
        {
            ModelState.AddModelError(nameof(model.TemplateCode), "Please select a valid form template.");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.FormBody))
        {
            model.FormBody = template.DefaultBody;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Controlled mapping prevents accidental over-posting.
        form.TemplateCode = template.Code;
        form.TemplateName = template.Name;
        form.ClientFullName = model.ClientFullName.Trim();
        form.ClientReferenceNumber = model.ClientReferenceNumber?.Trim();
        form.CaseReference = model.CaseReference.Trim();
        form.DepartmentDestination = model.DepartmentDestination.Trim();
        form.FormBody = model.FormBody;
        form.InternalNotes = model.InternalNotes;
        form.Status = model.Status;
        form.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var userId = User.GetUserId() ?? form.CreatedByUserId;
        var warnings = await SaveFormAttachmentsAsync(form.Id, userId, files);

        if (warnings.Count > 0)
        {
            TempData["WarningMessage"] = string.Join(" ", warnings);
        }

        TempData["SuccessMessage"] = "Form updated successfully.";
        return RedirectToAction(nameof(Details), new { id = form.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        return View(form);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        foreach (var attachment in form.Attachments)
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoragePath);
        }

        _context.SocialWorkForms.Remove(form);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Form deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Print(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        return View(form);
    }

    public async Task<IActionResult> DownloadSummary(int id)
    {
        var form = await LoadAuthorizedFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        var content = BuildFormSummary(form);
        var bytes = Encoding.UTF8.GetBytes(content);
        var fileName = $"SocialWorkForm_{form.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

        return File(bytes, "text/plain", fileName);
    }

    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var attachment = await _context.FormAttachments
            .Include(a => a.SocialWorkForm)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment is null || attachment.SocialWorkForm is null)
        {
            return NotFound();
        }

        if (!User.IsAdmin() && attachment.SocialWorkForm.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        var stream = await _fileStorageService.OpenReadAsync(attachment.StoragePath);
        return File(stream, attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int id, int formId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var attachment = await _context.FormAttachments
            .Include(a => a.SocialWorkForm)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment is null || attachment.SocialWorkForm is null)
        {
            return NotFound();
        }

        if (!User.IsAdmin() && attachment.SocialWorkForm.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        await _fileStorageService.DeleteFileAsync(attachment.StoragePath);

        _context.FormAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Attachment deleted.";
        return RedirectToAction(nameof(Details), new { id = formId });
    }

    private void PopulateLookups(SocialWorkFormInputViewModel model)
    {
        var templates = _templateService.GetTemplates();

        model.TemplateOptions = templates
            .Select(t => new SelectListItem
            {
                Value = t.Code,
                Text = $"{t.Code} - {t.Name}",
                Selected = string.Equals(t.Code, model.TemplateCode, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        model.TemplateBodies = templates.ToDictionary(t => t.Code, t => t.DefaultBody);

        model.Departments =
        [
            "Magistrate Court",
            "DOH",
            "DBE",
            "DSD",
            "Other"
        ];

        if (string.IsNullOrWhiteSpace(model.TemplateCode))
        {
            model.TemplateCode = templates.First().Code;
        }
    }

    private async Task<SocialWorkForm?> LoadAuthorizedFormAsync(int formId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return null;
        }

        var form = await _context.SocialWorkForms
            .Include(f => f.CreatedByUser)
            .Include(f => f.Attachments)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form is null)
        {
            return null;
        }

        if (!User.IsAdmin() && form.CreatedByUserId != userId.Value)
        {
            return null;
        }

        return form;
    }

    private async Task<List<string>> SaveFormAttachmentsAsync(int formId, int userId, List<IFormFile>? files)
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
                var stored = await _fileStorageService.SaveFileAsync(file, "forms");
                _context.FormAttachments.Add(new FormAttachment
                {
                    SocialWorkFormId = formId,
                    OriginalFileName = stored.OriginalFileName,
                    StoragePath = stored.StoragePath,
                    ContentType = stored.ContentType,
                    FileSizeBytes = stored.FileSizeBytes,
                    UploadedByUserId = userId,
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

    private static string BuildFormSummary(SocialWorkForm form)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"NGMHS SOCIAL WORK FORM #{form.Id}");
        sb.AppendLine($"Template: {form.TemplateCode} - {form.TemplateName}");
        sb.AppendLine($"Client: {form.ClientFullName}");
        sb.AppendLine($"Client Reference: {form.ClientReferenceNumber ?? "N/A"}");
        sb.AppendLine($"Case Reference: {form.CaseReference}");
        sb.AppendLine($"Department: {form.DepartmentDestination}");
        sb.AppendLine($"Status: {form.Status}");
        sb.AppendLine($"Created (UTC): {form.CreatedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Updated (UTC): {form.UpdatedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        sb.AppendLine("FORM BODY");
        sb.AppendLine("---------");
        sb.AppendLine(form.FormBody);

        if (!string.IsNullOrWhiteSpace(form.InternalNotes))
        {
            sb.AppendLine();
            sb.AppendLine("INTERNAL NOTES");
            sb.AppendLine("-------------");
            sb.AppendLine(form.InternalNotes);
        }

        return sb.ToString();
    }
}

