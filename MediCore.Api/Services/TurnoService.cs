using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Dtos.Turni;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services;

public class TurnoService(AppDbContext db) : ITurnoService
{
    public async Task<IReadOnlyList<TurnoResponse>> GetAllAsync() =>
        await Project(db.Turni.AsNoTracking()
            .OrderBy(t => t.GiornoSettimana).ThenBy(t => t.OraInizio)).ToListAsync();

    public async Task<IReadOnlyList<TurnoResponse>> GetByMedicoAsync(Guid medicoId) =>
        await Project(db.Turni.AsNoTracking()
            .Where(t => t.MedicoId == medicoId)
            .OrderBy(t => t.GiornoSettimana).ThenBy(t => t.OraInizio)).ToListAsync();

    public async Task<IReadOnlyList<TurnoResponse>> GetMieiAsync(string userId)
    {
        var medico = await db.Medici.FirstOrDefaultAsync(m => m.UserId == userId);
        if (medico is null)
            return [];

        return await Project(db.Turni.AsNoTracking()
            .Where(t => t.MedicoId == medico.MedicoId)
            .OrderBy(t => t.GiornoSettimana).ThenBy(t => t.OraInizio)).ToListAsync();
    }

    public async Task<TurnoResponse?> GetByIdAsync(Guid id) =>
        await Project(db.Turni.AsNoTracking().Where(t => t.TurnoId == id))
            .FirstOrDefaultAsync();

    public async Task<(EsitoOperazione Esito, TurnoResponse? Turno)> CreateAsync(TurnoRequest request)
    {
        if (request.OraFine <= request.OraInizio)
            return (EsitoOperazione.DatiNonValidi, null);

        var medico = await db.Medici.Include(m => m.User).FirstOrDefaultAsync(m => m.MedicoId == request.MedicoId);
        if (medico is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        var prestazione = await db.Prestazioni.FirstOrDefaultAsync(p => p.PrestazioneId == request.PrestazioneId);
        if (prestazione is null)
            return (EsitoOperazione.RiferimentoNonValido, null);

        if (await HaSovrapposizioneAsync(request.MedicoId, request.GiornoSettimana, request.OraInizio, request.OraFine, escludiTurnoId: null))
            return (EsitoOperazione.Conflitto, null);

        var turno = new Turno
        {
            MedicoId = request.MedicoId,
            PrestazioneId = request.PrestazioneId,
            GiornoSettimana = request.GiornoSettimana,
            OraInizio = request.OraInizio,
            OraFine = request.OraFine,
            DurataSlotMin = request.DurataSlotMin
        };

        db.Turni.Add(turno);
        await db.SaveChangesAsync();

        return (EsitoOperazione.Ok, ToResponse(turno, $"{medico.User.Nome} {medico.User.Cognome}", prestazione.Nome));
    }

    public async Task<EsitoOperazione> UpdateAsync(Guid id, TurnoRequest request)
    {
        if (request.OraFine <= request.OraInizio)
            return EsitoOperazione.DatiNonValidi;

        var turno = await db.Turni.FirstOrDefaultAsync(t => t.TurnoId == id);
        if (turno is null)
            return EsitoOperazione.NonTrovato;

        var medicoEsiste = await db.Medici.AnyAsync(m => m.MedicoId == request.MedicoId);
        if (!medicoEsiste)
            return EsitoOperazione.RiferimentoNonValido;

        var prestazioneEsiste = await db.Prestazioni.AnyAsync(p => p.PrestazioneId == request.PrestazioneId);
        if (!prestazioneEsiste)
            return EsitoOperazione.RiferimentoNonValido;

        if (await HaSovrapposizioneAsync(request.MedicoId, request.GiornoSettimana, request.OraInizio, request.OraFine, escludiTurnoId: id))
            return EsitoOperazione.Conflitto;

        turno.MedicoId = request.MedicoId;
        turno.PrestazioneId = request.PrestazioneId;
        turno.GiornoSettimana = request.GiornoSettimana;
        turno.OraInizio = request.OraInizio;
        turno.OraFine = request.OraFine;
        turno.DurataSlotMin = request.DurataSlotMin;
        await db.SaveChangesAsync();

        return EsitoOperazione.Ok;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var turno = await db.Turni.FirstOrDefaultAsync(t => t.TurnoId == id);
        if (turno is null)
            return false;

        db.Turni.Remove(turno);
        await db.SaveChangesAsync();
        return true;
    }

    private Task<bool> HaSovrapposizioneAsync(
        Guid medicoId,
        Domain.Enums.GiornoSettimana giornoSettimana,
        TimeOnly oraInizio,
        TimeOnly oraFine,
        Guid? escludiTurnoId)
    {
        var query = db.Turni.Where(t =>
            t.MedicoId == medicoId &&
            t.GiornoSettimana == giornoSettimana &&
            t.OraInizio < oraFine &&
            oraInizio < t.OraFine);

        if (escludiTurnoId is not null)
            query = query.Where(t => t.TurnoId != escludiTurnoId);

        return query.AnyAsync();
    }

    private static IQueryable<TurnoResponse> Project(IQueryable<Turno> query) =>
        query.Select(t => new TurnoResponse
        {
            Id = t.TurnoId,
            MedicoId = t.MedicoId,
            MedicoNomeCompleto = t.Medico.User.Nome + " " + t.Medico.User.Cognome,
            PrestazioneId = t.PrestazioneId,
            PrestazioneNome = t.Prestazione.Nome,
            GiornoSettimana = t.GiornoSettimana,
            OraInizio = t.OraInizio,
            OraFine = t.OraFine,
            DurataSlotMin = t.DurataSlotMin
        });

    private static TurnoResponse ToResponse(Turno t, string medicoNomeCompleto, string prestazioneNome) => new()
    {
        Id = t.TurnoId,
        MedicoId = t.MedicoId,
        MedicoNomeCompleto = medicoNomeCompleto,
        PrestazioneId = t.PrestazioneId,
        PrestazioneNome = prestazioneNome,
        GiornoSettimana = t.GiornoSettimana,
        OraInizio = t.OraInizio,
        OraFine = t.OraFine,
        DurataSlotMin = t.DurataSlotMin
    };
}
