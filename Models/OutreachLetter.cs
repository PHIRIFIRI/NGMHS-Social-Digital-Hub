using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class OutreachLetter
{
    public int Id { get; set; }

    public OutreachType OutreachType { get; set; } = OutreachType.Invitation;

    [Required]
    [MaxLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? RecipientEmail { get; set; }

    [MaxLength(200)]
    public string? RecipientOrganisation { get; set; }

    [Required]
    [MaxLength(250)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string LetterBody { get; set; } = string.Empty;

    public OutreachStatus Status { get; set; } = OutreachStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<OutreachAttachment> Attachments { get; set; } = new List<OutreachAttachment>();
}
