namespace MediCore.Api.Services.Storage;

// Salva i file su disco locale, sotto FileStorage:LocalPath (dev). In produzione la stessa
// interfaccia sarà implementata da AzureFileStorageService (Azure Blob, region Italy North).
public class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private readonly string _basePath = configuration["FileStorage:LocalPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "Referti");

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_basePath);

        var nomeFile = $"{Guid.CreateVersion7()}{Path.GetExtension(fileName)}";
        var percorsoCompleto = Path.Combine(_basePath, nomeFile);

        await using var destinazione = File.Create(percorsoCompleto);
        await content.CopyToAsync(destinazione, cancellationToken);

        return nomeFile;
    }

    public Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(File.OpenRead(Path.Combine(_basePath, filePath)));

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var percorsoCompleto = Path.Combine(_basePath, filePath);
        if (File.Exists(percorsoCompleto))
            File.Delete(percorsoCompleto);

        return Task.CompletedTask;
    }
}
