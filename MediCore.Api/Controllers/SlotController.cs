using MediCore.Api.Dtos.Slot;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("slot")]
[Authorize]
public class SlotController(ISlotService slotService) : ControllerBase
{
    [HttpGet("prestazione/{prestazioneId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SlotResponse>>> GetDisponibiliPerPrestazione(Guid prestazioneId)
    {
        var (esito, slot) = await slotService.GetDisponibiliPerPrestazioneAsync(prestazioneId);
        return esito switch
        {
            EsitoOperazione.Ok => Ok(slot),
            EsitoOperazione.NonTrovato => NotFound(),
            _ => BadRequest()
        };
    }
}
