using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NGMHS.Models;

namespace NGMHS.ViewModels;

public class SocialWorkFormInputViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Form Template")]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Display(Name = "Client Full Name")]
    public string ClientFullName { get; set; } = string.Empty;

    [MaxLength(30)]
    [Display(Name = "Client Reference Number")]
    public string? ClientReferenceNumber { get; set; }

    [Required]
    [MaxLength(120)]
    [Display(Name = "Case Reference")]
    public string CaseReference { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [Display(Name = "Destination Department")]
    public string DepartmentDestination { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Form Body")]
    public string FormBody { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Display(Name = "Internal Notes")]
    public string? InternalNotes { get; set; }

    public WorkFormStatus Status { get; set; } = WorkFormStatus.Active;

    public List<string> Departments { get; set; } = new();
    public List<SelectListItem> TemplateOptions { get; set; } = new();

    public Dictionary<string, string> TemplateBodies { get; set; } = new();
}
