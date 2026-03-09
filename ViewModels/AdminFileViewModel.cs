namespace NGMHS.ViewModels;

public class AdminFileViewModel
{
    public string Module { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public long SizeBytes { get; set; }
    public string DownloadController { get; set; } = string.Empty;
    public string DownloadAction { get; set; } = string.Empty;
    public int FileId { get; set; }
}
