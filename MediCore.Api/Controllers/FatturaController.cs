using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Fatture;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("fatture")]
[Authorize]
public class FatturaController(IFatturaService fatturaService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FatturaResponse>> GetById(Guid id)
    {
        var (esito, fattura) = await fatturaService.GetByIdAsync(id, UserId, IsAdmin);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(fattura),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpGet("mie")]
    public async Task<ActionResult<IReadOnlyList<FatturaResponse>>> GetMie() =>
        Ok(await fatturaService.GetMieAsync(UserId));

    [HttpGet]
    [Authorize(Roles = AppRoles.Amministratore)]
    public async Task<ActionResult<IReadOnlyList<FatturaResponse>>> GetAll() =>
        Ok(await fatturaService.GetAllAsync());

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(AppRoles.Amministratore);
}
