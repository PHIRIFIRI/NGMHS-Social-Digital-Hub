using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class SocialWorkForm
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ClientFullName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ClientReferenceNumber { get; set; }

    [Required]
    [MaxLength(120)]
    public string CaseReference { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string DepartmentDestination { get; set; } = string.Empty;

    [Required]
    public string FormBody { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? InternalNotes { get; set; }

    public WorkFormStatus Status { get; set; } = WorkFormStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<FormAttachment> Attachments { get; set; } = new List<FormAttachment>();
}
