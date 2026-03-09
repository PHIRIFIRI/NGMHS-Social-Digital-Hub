using Microsoft.AspNetCore.Http;

namespace NGMHS.Services;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveFileAsync(IFormFile file, string container, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
}
