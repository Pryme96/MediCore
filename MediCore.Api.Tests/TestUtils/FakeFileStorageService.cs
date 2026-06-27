using MediCore.Api.Services.Storage;

namespace MediCore.Api.Tests.TestUtils;

// Storage in memoria: evita di scrivere file su disco durante gli unit test.
public class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _file = new();

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        using var memoria = new MemoryStream();
        await content.CopyToAsync(memoria, cancellationToken);

        var percorso = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        _file[percorso] = memoria.ToArray();
        return percorso;
    }

    public Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(new MemoryStream(_file[filePath]));

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _file.Remove(filePath);
        return Task.CompletedTask;
    }

    public bool Esiste(string filePath) => _file.ContainsKey(filePath);
}
