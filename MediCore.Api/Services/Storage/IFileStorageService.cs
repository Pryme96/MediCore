namespace MediCore.Api.Services.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
