using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Dtos.Slot;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class SlotService(AppDbContext db, IConfiguration configuration) : ISlotService
{
    public async Task<(EsitoOperazione Esito, IReadOnlyList<SlotResponse>? Slot)> GetDisponibiliPerPrestazioneAsync(Guid prestazioneId)
    {
        var prestazioneEsiste = await db.Prestazioni.AnyAsync(p => p.PrestazioneId == prestazioneId);
        if (!prestazioneEsiste)
            return (EsitoOperazione.NonTrovato, null);

        var turni = await db.Turni.Where(t => t.PrestazioneId == prestazioneId).ToListAsync();
        if (turni.Count > 0)
            await GeneraSlotMancantiAsync(turni);

        var slot = await db.Slot.AsNoTracking()
            .Where(s => s.Turno.PrestazioneId == prestazioneId
                && s.Stato == StatoSlot.Libero
                && s.DataOraInizio >= DateTime.Now)
            .OrderBy(s => s.DataOraInizio)
            .Select(s => new SlotResponse
            {
                Id = s.SlotId,
                TurnoId = s.TurnoId,
                MedicoId = s.Turno.MedicoId,
                MedicoNomeCompleto = s.Turno.Medico.User.Nome + " " + s.Turno.Medico.User.Cognome,
                DataOraInizio = s.DataOraInizio,
                DataOraFine = s.DataOraFine
            })
            .ToListAsync();

        return (EsitoOperazione.Ok, slot);
    }

    private async Task GeneraSlotMancantiAsync(List<Turno> turni)
    {
        var giorniFinestra = configuration.GetValue<int?>("SlotGeneration:GiorniFinestra") ?? 60;
        var oggi = DateTime.Now.Date;
        var ultimoGiorno = oggi.AddDays(giorniFinestra);

        var turnoIds = turni.Select(t => t.TurnoId).ToList();
        var esistenti = await db.Slot
            .Where(s => turnoIds.Contains(s.TurnoId) && s.DataOraInizio >= oggi)
            .Select(s => new { s.TurnoId, s.DataOraInizio })
            .ToListAsync();
        var esistentiSet = esistenti.Select(e => (e.TurnoId, e.DataOraInizio)).ToHashSet();

        var nuoviSlot = new List<Slot>();
        for (var data = oggi; data <= ultimoGiorno; data = data.AddDays(1))
        {
            var giornoSettimana = DataToGiornoSettimana(data.DayOfWeek);

            foreach (var turno in turni.Where(t => t.GiornoSettimana == giornoSettimana))
            {
                for (var oraInizio = turno.OraInizio;
                     oraInizio.AddMinutes(turno.DurataSlotMin) <= turno.OraFine;
                     oraInizio = oraInizio.AddMinutes(turno.DurataSlotMin))
                {
                    var dataOraInizio = data.Add(oraInizio.ToTimeSpan());
                    if (dataOraInizio < DateTime.Now)
                        continue;

                    if (esistentiSet.Contains((turno.TurnoId, dataOraInizio)))
                        continue;

                    nuoviSlot.Add(new Slot
                    {
                        TurnoId = turno.TurnoId,
                        DataOraInizio = dataOraInizio,
                        DataOraFine = data.Add(oraInizio.AddMinutes(turno.DurataSlotMin).ToTimeSpan()),
                        Stato = StatoSlot.Libero
                    });
                }
            }
        }

        if (nuoviSlot.Count == 0)
            return;

        db.Slot.AddRange(nuoviSlot);
        await db.SaveChangesAsync();
    }

    private static GiornoSettimana DataToGiornoSettimana(DayOfWeek giorno) =>
        (GiornoSettimana)(giorno == DayOfWeek.Sunday ? 7 : (int)giorno);
}
