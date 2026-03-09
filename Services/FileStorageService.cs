using Microsoft.AspNetCore.Http;

namespace NGMHS.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".doc", ".docx"
    ];

    private readonly string _storageRoot;
    private readonly long _maxFileSizeBytes;

    public FileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configuredRoot = configuration["Storage:RootPath"] ?? "Storage";
        _storageRoot = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        var maxSizeMb = configuration.GetValue<long?>("Storage:MaxFileSizeMb") ?? 10;
        _maxFileSizeBytes = maxSizeMb * 1024 * 1024;

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<StoredFileResult> SaveFileAsync(IFormFile file, string container, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if (file.Length > _maxFileSizeBytes)
        {
            throw new InvalidOperationException($"File is too large. Maximum size is {_maxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("File type is not allowed. Upload PDF or image/office documents only.");
        }

        var safeContainer = container.Trim().Replace("..", string.Empty);
        var containerPath = Path.Combine(_storageRoot, safeContainer);
        Directory.CreateDirectory(containerPath);

        var generatedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(containerPath, generatedName);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine(safeContainer, generatedName).Replace('\\', '/');
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        return new StoredFileResult(relativePath, Path.GetFileName(file.FileName), contentType, file.Length);
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The requested file was not found.", storagePath);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string storagePath)
    {
        var normalized = storagePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, normalized));
        var rootFullPath = Path.GetFullPath(_storageRoot);

        if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return fullPath;
    }
}
