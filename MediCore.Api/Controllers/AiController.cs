using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Ai;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("ai")]
[Authorize]
public class AiController(ISuggerimentoService suggerimentoService) : ControllerBase
{
    // Assistenza alla redazione (HITL): propone bozze, non scrive nulla sul DB.
    [HttpPost("suggerisci")]
    [Authorize(Roles = AppRoles.Medico)]
    public async Task<ActionResult<SuggerimentoResponse>> Suggerisci(SuggerimentoRequest request)
    {
        var (esito, risposta) = await suggerimentoService.SuggerisciAsync(request, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(risposta),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il paziente indicato non esiste o non ha mai avuto una prenotazione con questo medico."),
            EsitoOperazione.DatiNonValidi => BadRequest("Indicare il contesto clinico per ottenere dei suggerimenti."),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
