using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("tariffe")]
[Authorize]
public class TariffaController(ITariffaService tariffaService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TariffaResponse>> GetById(Guid id)
    {
        var tariffa = await tariffaService.GetByIdAsync(id);
        return tariffa is null ? NotFound() : Ok(tariffa);
    }

    [HttpGet("prestazione/{prestazioneId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TariffaResponse>>> GetByPrestazione(Guid prestazioneId) =>
        Ok(await tariffaService.GetByPrestazioneAsync(prestazioneId));

    [HttpPost]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<TariffaResponse>> Create(TariffaRequest request)
    {
        var (esito, tariffa) = await tariffaService.CreateAsync(request);
        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = tariffa!.Id }, tariffa),
            EsitoOperazione.RiferimentoNonValido => BadRequest("La prestazione indicata non esiste."),
            EsitoOperazione.Conflitto => Conflict("Esiste già una tariffa per questa prestazione e regime."),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Update(Guid id, TariffaRequest request) =>
        await tariffaService.UpdateAsync(id, request) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.RiferimentoNonValido => BadRequest("La prestazione indicata non esiste."),
            EsitoOperazione.Conflitto => Conflict("Esiste già una tariffa per questa prestazione e regime."),
            _ => BadRequest()
        };

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Delete(Guid id) =>
        await tariffaService.DeleteAsync(id) ? NoContent() : NotFound();
}
