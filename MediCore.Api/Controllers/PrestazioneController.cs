using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("prestazioni")]
[Authorize]
public class PrestazioneController(IPrestazioneService prestazioneService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PrestazioneResponse>>> GetAll() =>
        Ok(await prestazioneService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PrestazioneResponse>> GetById(Guid id)
    {
        var prestazione = await prestazioneService.GetByIdAsync(id);
        return prestazione is null ? NotFound() : Ok(prestazione);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<PrestazioneResponse>> Create(PrestazioneRequest request)
    {
        var created = await prestazioneService.CreateAsync(request);
        if (created is null)
            return BadRequest("Il servizio indicato non esiste.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Update(Guid id, PrestazioneRequest request) =>
        await prestazioneService.UpdateAsync(id, request) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Il servizio indicato non esiste."),
            _ => BadRequest()
        };
}
