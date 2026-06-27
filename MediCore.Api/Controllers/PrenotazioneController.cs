using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Prenotazioni;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("prenotazioni")]
[Authorize]
public class PrenotazioneController(IPrenotazioneService prenotazioneService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PrenotazioneResponse>> Create(PrenotazioneRequest request)
    {
        var (esito, prenotazione) = await prenotazioneService.CreateAsync(request, UserId, IsAdmin);
        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = prenotazione!.Id }, prenotazione),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Lo slot o il paziente indicati non esistono."),
            EsitoOperazione.Conflitto => Conflict("Lo slot non è più disponibile."),
            EsitoOperazione.DatiNonValidi => BadRequest("È necessario indicare il paziente per cui si prenota."),
            _ => BadRequest()
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PrenotazioneResponse>> GetById(Guid id)
    {
        var (esito, prenotazione) = await prenotazioneService.GetByIdAsync(id, UserId, IsAdmin);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(prenotazione),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpGet("mie")]
    public async Task<ActionResult<IReadOnlyList<PrenotazioneResponse>>> GetMie() =>
        Ok(await prenotazioneService.GetMieAsync(UserId));

    [HttpPut("{id:guid}/annulla")]
    public async Task<IActionResult> Annulla(Guid id) =>
        await prenotazioneService.AnnullaAsync(id, UserId, IsAdmin) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            EsitoOperazione.Conflitto => Conflict("La prenotazione non è più annullabile."),
            _ => BadRequest()
        };

    [HttpPut("{id:guid}/completa")]
    public async Task<IActionResult> Completa(Guid id) =>
        await prenotazioneService.CompletaAsync(id, UserId, IsAdmin) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            EsitoOperazione.Conflitto => Conflict("La prenotazione non è completabile."),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Nessuna tariffa configurata per questa prestazione e questo regime."),
            _ => BadRequest()
        };

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(AppRoles.Amministratore);
}
