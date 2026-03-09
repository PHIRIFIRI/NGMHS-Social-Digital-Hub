using System.ComponentModel.DataAnnotations;

namespace NGMHS.ViewModels;

public class ExternalQueryCreateViewModel
{
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
}
