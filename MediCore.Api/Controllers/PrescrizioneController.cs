using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Prescrizioni;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("prescrizioni")]
[Authorize]
public class PrescrizioneController(IPrescrizioneService prescrizioneService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = AppRoles.Medico)]
    public async Task<ActionResult<PrescrizioneResponse>> Create(PrescrizioneRequest request)
    {
        var (esito, prescrizione) = await prescrizioneService.CreateAsync(request, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = prescrizione!.Id }, prescrizione),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il paziente indicato non esiste o non ha mai avuto una prenotazione con questo medico."),
            EsitoOperazione.DatiNonValidi => BadRequest("Dati della prescrizione non validi: verifica le date, almeno una riga con farmaco/posologia/quantità e, per il piano terapeutico, la diagnosi."),
            _ => BadRequest()
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PrescrizioneResponse>> GetById(Guid id)
    {
        var (esito, prescrizione) = await prescrizioneService.GetByIdAsync(id, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(prescrizione),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpGet("mie")]
    public async Task<ActionResult<IReadOnlyList<PrescrizioneResponse>>> GetMie() =>
        Ok(await prescrizioneService.GetMieAsync(UserId));

    [HttpGet("emesse")]
    [Authorize(Roles = AppRoles.Medico)]
    public async Task<ActionResult<IReadOnlyList<PrescrizioneResponse>>> GetEmesse() =>
        Ok(await prescrizioneService.GetEmesseAsync(UserId));

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
