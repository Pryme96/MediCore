using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Medici;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("medici")]
[Authorize]
public class MedicoController(IMedicoService medicoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicoResponse>>> GetAll() =>
        Ok(await medicoService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MedicoResponse>> GetById(Guid id)
    {
        var medico = await medicoService.GetByIdAsync(id);
        return medico is null ? NotFound() : Ok(medico);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<MedicoCreatoResponse>> Create(MedicoRequest request)
    {
        var (esito, medico) = await medicoService.CreateAsync(request);
        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = medico!.Medico.Id }, medico),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il servizio indicato non esiste."),
            EsitoOperazione.Conflitto => Conflict("Esiste già un account con questa email."),
            EsitoOperazione.DatiNonValidi => BadRequest("Impossibile creare l'account utente per il medico."),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Update(Guid id, MedicoUpdateRequest request) =>
        await medicoService.UpdateAsync(id, request) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il servizio indicato non esiste."),
            _ => BadRequest()
        };

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<PasswordResetResponse>> ResetPassword(Guid id)
    {
        var (esito, risultato) = await medicoService.ResetPasswordAsync(id);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(risultato),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.DatiNonValidi => BadRequest("Impossibile reimpostare la password del medico."),
            _ => BadRequest()
        };
    }
}
