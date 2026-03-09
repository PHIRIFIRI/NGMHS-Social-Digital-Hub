using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class FormAttachment
{
    public int Id { get; set; }

    [Required]
    public int SocialWorkFormId { get; set; }

    [Required]
    [MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(350)]
    public string StoragePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public SocialWorkForm? SocialWorkForm { get; set; }
}
