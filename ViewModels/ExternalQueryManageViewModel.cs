using System.ComponentModel.DataAnnotations;
using NGMHS.Models;

namespace NGMHS.ViewModels;

public class ExternalQueryManageViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? OrganisationName { get; set; }
    public string QuerySubject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Current Status")]
    public ExternalQueryStatus Status { get; set; }

    [Display(Name = "Response Notes")]
    [MaxLength(2000)]
    public string? ResponseNotes { get; set; }

    [Display(Name = "Assigned Social Worker")]
    public int? AssignedToUserId { get; set; }
}
