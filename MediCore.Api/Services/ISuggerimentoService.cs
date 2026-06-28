using MediCore.Api.Dtos.Ai;

namespace MediCore.Api.Services;

public interface ISuggerimentoService
{
    Task<(EsitoOperazione Esito, SuggerimentoResponse? Risposta)> SuggerisciAsync(SuggerimentoRequest request, string userId);
}
