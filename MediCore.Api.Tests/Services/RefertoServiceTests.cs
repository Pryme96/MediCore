using System.Text;
using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using MediCore.Api.Services;
using MediCore.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Tests.Services;

public class RefertoServiceTests
{
    private static async Task<(AppDbContext Db, Medico Medico, Medico AltroMedico, Prenotazione Prenotazione)> SetupAsync()
    {
        var db = AppDbContextFactory.Create();

        var servizio = new Servizio { Nome = "Cardiologia", Descrizione = "Servizio di cardiologia" };
        var prestazione = new Prestazione
        {
            ServizioId = servizio.ServizioId,
            Nome = "Visita cardiologica",
            Descrizione = "Prima visita",
            DurataMinuti = 30
        };
        var medicoUser = new AppUser { UserName = "medico@medicore.local", Email = "medico@medicore.local", Nome = "Mario", Cognome = "Rossi" };
        var medico = new Medico { UserId = medicoUser.Id, Specializzazione = "Cardiologia", ServizioId = servizio.ServizioId };
        var altroMedicoUser = new AppUser { UserName = "altro@medicore.local", Email = "altro@medicore.local", Nome = "Anna", Cognome = "Verdi" };
        var altroMedico = new Medico { UserId = altroMedicoUser.Id, Specializzazione = "Cardiologia", ServizioId = servizio.ServizioId };

        var turno = new Turno
        {
            MedicoId = medico.MedicoId,
            PrestazioneId = prestazione.PrestazioneId,
            GiornoSettimana = GiornoSettimana.Lunedi,
            OraInizio = new TimeOnly(9, 0),
            OraFine = new TimeOnly(12, 0),
            DurataSlotMin = 30
        };
        var slot = new Slot
        {
            TurnoId = turno.TurnoId,
            DataOraInizio = DateTime.Now.Date.AddDays(-7).AddHours(9),
            DataOraFine = DateTime.Now.Date.AddDays(-7).AddHours(9).AddMinutes(30),
            Stato = StatoSlot.Prenotato
        };
        var pazienteUser = new AppUser { UserName = "paziente@medicore.local", Email = "paziente@medicore.local", Nome = "Luca", Cognome = "Bianchi" };
        var paziente = new Paziente
        {
            UserId = pazienteUser.Id,
            CodiceFiscale = "BNCLCU90A01H501A",
            DataNascita = new DateOnly(1990, 1, 1),
            Telefono = "3331234567"
        };
        var prenotazione = new Prenotazione
        {
            PazienteId = paziente.PazienteId,
            SlotId = slot.SlotId,
            Regime = Regime.Ssn,
            Stato = StatoPrenotazione.Completata
        };

        db.Servizi.Add(servizio);
        db.Prestazioni.Add(prestazione);
        db.Users.AddRange(medicoUser, altroMedicoUser, pazienteUser);
        db.Medici.AddRange(medico, altroMedico);
        db.Pazienti.Add(paziente);
        db.Turni.Add(turno);
        db.Slot.Add(slot);
        db.Prenotazioni.Add(prenotazione);
        await db.SaveChangesAsync();

        return (db, medico, altroMedico, prenotazione);
    }

    private static Stream PdfFinto() => new MemoryStream(Encoding.UTF8.GetBytes("%PDF-1.4 contenuto finto"));

    [Fact]
    public async Task UploadAsync_medico_proprietario_restituisce_Ok()
    {
        var (db, medico, _, prenotazione) = await SetupAsync();
        var storage = new FakeFileStorageService();
        var service = new RefertoService(db, storage);

        var (esito, referto) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "referto.pdf", "application/pdf", "Esito nella norma", medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(referto);
        Assert.Equal("Esito nella norma", referto!.Contenuto);
    }

    [Fact]
    public async Task UploadAsync_con_contentType_non_pdf_restituisce_DatiNonValidi()
    {
        var (db, medico, _, prenotazione) = await SetupAsync();
        var service = new RefertoService(db, new FakeFileStorageService());

        var (esito, referto) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "referto.png", "image/png", null, medico.UserId);

        Assert.Equal(EsitoOperazione.DatiNonValidi, esito);
        Assert.Null(referto);
    }

    [Fact]
    public async Task UploadAsync_con_prenotazione_inesistente_restituisce_NonTrovato()
    {
        var (db, medico, _, _) = await SetupAsync();
        var service = new RefertoService(db, new FakeFileStorageService());

        var (esito, referto) = await service.UploadAsync(
            Guid.NewGuid(), PdfFinto(), "referto.pdf", "application/pdf", null, medico.UserId);

        Assert.Equal(EsitoOperazione.NonTrovato, esito);
        Assert.Null(referto);
    }

    [Fact]
    public async Task UploadAsync_da_medico_non_proprietario_della_prenotazione_restituisce_NonAutorizzato()
    {
        var (db, _, altroMedico, prenotazione) = await SetupAsync();
        var service = new RefertoService(db, new FakeFileStorageService());

        var (esito, referto) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "referto.pdf", "application/pdf", null, altroMedico.UserId);

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(referto);
    }

    [Fact]
    public async Task UploadAsync_su_prenotazione_con_referto_esistente_sovrascrive_il_file_precedente()
    {
        var (db, medico, _, prenotazione) = await SetupAsync();
        var storage = new FakeFileStorageService();
        var service = new RefertoService(db, storage);

        var (_, primo) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "v1.pdf", "application/pdf", "Versione 1", medico.UserId);
        var primoPercorso = (await db.Referti.FindAsync(primo!.Id))!.FilePath!;

        var (esito, secondo) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "v2.pdf", "application/pdf", "Versione 2", medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.Equal(primo.Id, secondo!.Id);
        Assert.Equal("Versione 2", secondo.Contenuto);
        Assert.False(storage.Esiste(primoPercorso));
        Assert.Equal(1, await db.Referti.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_utente_estraneo_restituisce_NonAutorizzato()
    {
        var (db, medico, altroMedico, prenotazione) = await SetupAsync();
        var service = new RefertoService(db, new FakeFileStorageService());
        var (_, creato) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "referto.pdf", "application/pdf", null, medico.UserId);

        var (esito, referto) = await service.GetByIdAsync(creato!.Id, altroMedico.UserId);

        Assert.Equal(EsitoOperazione.NonAutorizzato, esito);
        Assert.Null(referto);
    }

    [Fact]
    public async Task DownloadAsync_restituisce_lo_stream_caricato()
    {
        var (db, medico, _, prenotazione) = await SetupAsync();
        var service = new RefertoService(db, new FakeFileStorageService());
        var (_, creato) = await service.UploadAsync(
            prenotazione.PrenotazioneId, PdfFinto(), "referto.pdf", "application/pdf", null, medico.UserId);

        var (esito, contenuto, nomeFile) = await service.DownloadAsync(creato!.Id, medico.UserId);

        Assert.Equal(EsitoOperazione.Ok, esito);
        Assert.NotNull(contenuto);
        Assert.NotNull(nomeFile);
    }
}
