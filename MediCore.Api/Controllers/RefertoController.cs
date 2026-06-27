using System.Security.Claims;
using MediCore.Api.Domain.Common;
using MediCore.Api.Dtos.Referti;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("referti")]
[Authorize]
public class RefertoController(IRefertoService refertoService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = AppRoles.Medico)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<RefertoResponse>> Upload([FromForm] RefertoUploadRequest request)
    {
        await using var stream = request.File.OpenReadStream();
        var (esito, referto) = await refertoService.UploadAsync(
            request.PrenotazioneId, stream, request.File.FileName, request.File.ContentType, request.Contenuto, UserId);

        return esito switch
        {
            EsitoOperazione.Ok => CreatedAtAction(nameof(GetById), new { id = referto!.Id }, referto),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.RiferimentoNonValido => BadRequest("Profilo medico non trovato."),
            EsitoOperazione.NonAutorizzato => Forbid(),
            EsitoOperazione.DatiNonValidi => BadRequest("Il file deve essere un PDF."),
            _ => BadRequest()
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RefertoResponse>> GetById(Guid id)
    {
        var (esito, referto) = await refertoService.GetByIdAsync(id, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(referto),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpGet("prenotazione/{prenotazioneId:guid}")]
    public async Task<ActionResult<RefertoResponse>> GetByPrenotazione(Guid prenotazioneId)
    {
        var (esito, referto) = await refertoService.GetByPrenotazioneAsync(prenotazioneId, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(referto),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> Download(Guid id)
    {
        var (esito, contenuto, nomeFile) = await refertoService.DownloadAsync(id, UserId);
        return esito switch
        {
            EsitoOperazione.Ok => File(contenuto!, "application/pdf", nomeFile),
            EsitoOperazione.NonTrovato => NotFound(),
            EsitoOperazione.NonAutorizzato => Forbid(),
            _ => BadRequest()
        };
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
