namespace NGMHS.Services;

public record StoredFileResult(
    string StoragePath,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes);
