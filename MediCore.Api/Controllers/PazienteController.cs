using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Pazienti;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("pazienti")]
[Authorize(Roles = $"{AppRoles.Amministratore},{AppRoles.Medico}")]
public class PazienteController(IPazienteService pazienteService) : ControllerBase
{
    // Lista dei pazienti registrati, usata dagli operatori per prenotare per conto del paziente.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PazienteResponse>>> GetAll() =>
        Ok(await pazienteService.GetAllAsync());
}
