using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Catalogo;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("servizi")]
[Authorize]
public class ServizioController(
    IServizioService servizioService,
    IPrestazioneService prestazioneService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServizioResponse>>> GetAll() =>
        Ok(await servizioService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServizioResponse>> GetById(Guid id)
    {
        var servizio = await servizioService.GetByIdAsync(id);
        return servizio is null ? NotFound() : Ok(servizio);
    }

    [HttpGet("{id:guid}/prestazioni")]
    public async Task<ActionResult<IReadOnlyList<PrestazioneResponse>>> GetPrestazioni(Guid id)
    {
        if (await servizioService.GetByIdAsync(id) is null)
            return NotFound();

        return Ok(await prestazioneService.GetByServizioAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<ServizioResponse>> Create(ServizioRequest request)
    {
        var created = await servizioService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<IActionResult> Update(Guid id, ServizioRequest request)
    {
        var updated = await servizioService.UpdateAsync(id, request);
        return updated ? NoContent() : NotFound();
    }
}
