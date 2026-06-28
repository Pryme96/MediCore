using MediCore.Api.Dtos.Prenotazioni;

namespace MediCore.Api.Services;

public interface IPrenotazioneService
{
    Task<(EsitoOperazione Esito, PrenotazioneResponse? Prenotazione)> CreateAsync(PrenotazioneRequest request, string userId, bool puoPrenotarePerAltri);
    Task<(EsitoOperazione Esito, PrenotazioneResponse? Prenotazione)> GetByIdAsync(Guid id, string userId, bool isAdmin);
    Task<IReadOnlyList<PrenotazioneResponse>> GetMieAsync(string userId);
    Task<IReadOnlyList<PrenotazioneResponse>> GetAgendaMedicoAsync(string userId);
    Task<EsitoOperazione> AnnullaAsync(Guid id, string userId, bool isAdmin);
    Task<EsitoOperazione> CompletaAsync(Guid id, string userId, bool isAdmin);
}
