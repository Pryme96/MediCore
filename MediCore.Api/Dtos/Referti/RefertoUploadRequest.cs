using Microsoft.AspNetCore.Http;

namespace MediCore.Api.Dtos.Referti;

public record RefertoUploadRequest
{
    public Guid PrenotazioneId { get; init; }
    public IFormFile File { get; init; } = null!;
    public string? Contenuto { get; init; }
}
