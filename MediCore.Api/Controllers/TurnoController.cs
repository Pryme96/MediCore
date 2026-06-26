using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Turni;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("turni")]
[Authorize]
public class TurnoController(ITurnoService turnoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TurnoResponse>>> GetAll() =>
        Ok(await turnoService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TurnoResponse>> GetById(Guid id)
    {
        var turno = await turnoService.GetByIdAsync(id);
        return turno is null ? NotFound() : Ok(turno);
    }

    [HttpGet("medico/{medicoId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TurnoResponse>>> GetByMedico(Guid medicoId) =>
        Ok(await turnoService.GetByMedicoAsync(medicoId));

    [HttpPost]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<TurnoResponse>> Create(TurnoRequest request)
    {
        var (esito, turno) = await turnoService.CreateAsync(request);
        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = turno!.Id }, turno),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il medico o la prestazione indicati non esistono."),
            EsitoOperazione.Conflitto => Conflict("Il medico ha già un turno sovrapposto in questa fascia."),
            EsitoOperazione.DatiNonValidi => BadRequest("L'ora di fine deve essere successiva all'ora di inizio."),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Update(Guid id, TurnoRequest request) =>
        await turnoService.UpdateAsync(id, request) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il medico o la prestazione indicati non esistono."),
            EsitoOperazione.Conflitto => Conflict("Il medico ha già un turno sovrapposto in questa fascia."),
            EsitoOperazione.DatiNonValidi => BadRequest("L'ora di fine deve essere successiva all'ora di inizio."),
            _ => BadRequest()
        };

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Delete(Guid id) =>
        await turnoService.DeleteAsync(id) ? NoContent() : NotFound();
}
