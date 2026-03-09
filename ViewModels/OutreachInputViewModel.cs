using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NGMHS.Models;

namespace NGMHS.ViewModels;

public class OutreachInputViewModel
{
    public int Id { get; set; }

    [Display(Name = "Outreach Type")]
    public OutreachType OutreachType { get; set; } = OutreachType.Invitation;

    [Required]
    [MaxLength(150)]
    [Display(Name = "Recipient Name")]
    public string RecipientName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    [Display(Name = "Recipient Email")]
    public string? RecipientEmail { get; set; }

    [MaxLength(200)]
    [Display(Name = "Recipient Organisation")]
    public string? RecipientOrganisation { get; set; }

    [Required]
    [MaxLength(250)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Letter Body")]
    public string LetterBody { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public OutreachStatus Status { get; set; } = OutreachStatus.Draft;

    public List<SelectListItem> OutreachTypeOptions { get; set; } = new();
}
