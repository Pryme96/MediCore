using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Notifiche;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("notifiche")]
[Authorize]
public class NotificaController(INotificaService notificaService) : ControllerBase
{
    [HttpGet("mie")]
    public async Task<ActionResult<IReadOnlyList<NotificaResponse>>> GetMie() =>
        Ok(await notificaService.GetMieAsync(UserId));

    [HttpGet("non-lette/count")]
    public async Task<ActionResult<int>> ContaNonLette() =>
        Ok(await notificaService.ContaNonLetteAsync(UserId));

    [HttpPut("{id:guid}/letta")]
    public async Task<IActionResult> MarcaLetta(Guid id) =>
        await notificaService.MarcaLettaAsync(id, UserId) switch
        {
            EsitoOperazione.Ok => NoContent(),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };

    // Generazione on-demand dei promemoria (oltre al worker periodico): utile per la demo e per
    // i test di integrazione deterministici. Riservata all'Amministratore.
    [HttpPost("genera-promemoria")]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<int>> GeneraPromemoria() =>
        Ok(await notificaService.GeneraPromemoriaDovutiAsync());

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
