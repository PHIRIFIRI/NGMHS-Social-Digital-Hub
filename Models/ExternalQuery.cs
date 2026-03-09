using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class ExternalQuery
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(200)]
    public string? OrganisationName { get; set; }

    [Required]
    [MaxLength(250)]
    public string QuerySubject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public ExternalQueryStatus Status { get; set; } = ExternalQueryStatus.New;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? ResponseNotes { get; set; }

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}
