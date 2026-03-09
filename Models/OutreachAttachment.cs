using System.ComponentModel.DataAnnotations;

namespace NGMHS.Models;

public class OutreachAttachment
{
    public int Id { get; set; }

    [Required]
    public int OutreachLetterId { get; set; }

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

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public OutreachLetter? OutreachLetter { get; set; }
}
