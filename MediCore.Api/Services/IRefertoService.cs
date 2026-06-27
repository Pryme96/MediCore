using MediCore.Api.Dtos.Referti;

namespace MediCore.Api.Services;

public interface IRefertoService
{
    Task<(EsitoOperazione Esito, RefertoResponse? Referto)> UploadAsync(
        Guid prenotazioneId, Stream fileContent, string fileName, string contentType, string? contenuto, string userId);
    Task<(EsitoOperazione Esito, RefertoResponse? Referto)> GetByIdAsync(Guid id, string userId);
    Task<(EsitoOperazione Esito, RefertoResponse? Referto)> GetByPrenotazioneAsync(Guid prenotazioneId, string userId);
    Task<(EsitoOperazione Esito, Stream? Contenuto, string? NomeFile)> DownloadAsync(Guid id, string userId);
}
