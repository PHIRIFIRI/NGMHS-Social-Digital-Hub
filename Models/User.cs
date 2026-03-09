using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "SocialWorker";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SocialWorkForm> CreatedForms { get; set; } = new List<SocialWorkForm>();
    public ICollection<FormAttachment> UploadedFormAttachments { get; set; } = new List<FormAttachment>();
    public ICollection<OutreachLetter> CreatedOutreachLetters { get; set; } = new List<OutreachLetter>();
}
