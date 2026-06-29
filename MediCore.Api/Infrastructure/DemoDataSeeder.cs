using MediCore.Api.Data;
using MediCore.Api.Domain.Common;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Infrastructure;

// Popola il database con dati dimostrativi realistici (catalogo, medici, turni, pazienti) per le
// sessioni di demo e gli screenshot. Idempotente: se il catalogo è già presente non fa nulla.
// Attivato solo quando DemoData:Enabled è true (vedi Program.cs); il profilo dei test lo disabilita,
// così il database di test resta pulito. Gli slot non vengono seedati: nascono lazy alla ricerca.
public static class DemoDataSeeder
{
    // Password note, uguali per categoria, pensate per accedere facilmente in fase di demo.
    private const string PasswordMedici = "Medico123!";
    private const string PasswordPazienti = "Paziente123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        // Se esiste già un catalogo si assume che i dati demo siano stati inseriti: non si duplica.
        if (await db.Servizi.AnyAsync())
            return;

        // --- Catalogo: servizi, prestazioni e tariffe per regime ---

        static Tariffa Tar(Regime regime, decimal prezzo) => new() { Regime = regime, Prezzo = prezzo };

        static Prestazione Pre(string nome, string descrizione, int durataMinuti, params Tariffa[] tariffe)
        {
            var prestazione = new Prestazione { Nome = nome, Descrizione = descrizione, DurataMinuti = durataMinuti };
            foreach (var tariffa in tariffe)
                prestazione.Tariffe.Add(tariffa);
            return prestazione;
        }

        static Servizio Srv(string nome, string descrizione, params Prestazione[] prestazioni)
        {
            var servizio = new Servizio { Nome = nome, Descrizione = descrizione };
            foreach (var prestazione in prestazioni)
                servizio.Prestazioni.Add(prestazione);
            return servizio;
        }

        var visitaCardiologica = Pre("Visita cardiologica", "Visita specialistica con anamnesi ed esame obiettivo cardiovascolare.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 120m), Tar(Regime.Assicurativo, 110m));
        var ecg = Pre("Elettrocardiogramma (ECG)", "Registrazione dell'attività elettrica del cuore a riposo.", 20,
            Tar(Regime.Ssn, 18m), Tar(Regime.Privato, 60m));
        var ecocardiogramma = Pre("Ecocardiogramma color doppler", "Ecografia del cuore con valutazione dei flussi.", 40,
            Tar(Regime.Ssn, 36m), Tar(Regime.Privato, 130m));
        var cardiologia = Srv("Cardiologia", "Diagnosi e cura delle malattie del cuore e dell'apparato cardiovascolare.",
            visitaCardiologica, ecg, ecocardiogramma);

        var visitaDermatologica = Pre("Visita dermatologica", "Visita specialistica della cute e degli annessi cutanei.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 110m), Tar(Regime.Assicurativo, 100m));
        var mappaturaNei = Pre("Mappatura dei nei in dermatoscopia", "Esame dei nei in epiluminescenza per il monitoraggio.", 30,
            Tar(Regime.Ssn, 30m), Tar(Regime.Privato, 130m));
        var dermatologia = Srv("Dermatologia", "Prevenzione, diagnosi e cura delle patologie della pelle.",
            visitaDermatologica, mappaturaNei);

        var visitaOrtopedica = Pre("Visita ortopedica", "Valutazione di patologie di ossa, articolazioni e muscoli.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 120m), Tar(Regime.Assicurativo, 110m));
        var infiltrazione = Pre("Infiltrazione articolare", "Iniezione intra-articolare a scopo terapeutico.", 20,
            Tar(Regime.Ssn, 20m), Tar(Regime.Privato, 90m));
        var ortopedia = Srv("Ortopedia", "Diagnosi e trattamento delle patologie dell'apparato muscolo-scheletrico.",
            visitaOrtopedica, infiltrazione);

        var visitaGinecologica = Pre("Visita ginecologica", "Visita specialistica dell'apparato genitale femminile.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 120m), Tar(Regime.Assicurativo, 110m));
        var ecografiaTransvaginale = Pre("Ecografia transvaginale", "Ecografia pelvica per la valutazione degli organi interni.", 30,
            Tar(Regime.Ssn, 36m), Tar(Regime.Privato, 100m));
        var ginecologia = Srv("Ginecologia", "Salute dell'apparato riproduttivo femminile e prevenzione.",
            visitaGinecologica, ecografiaTransvaginale);

        var visitaOtorino = Pre("Visita otorinolaringoiatrica", "Valutazione di orecchio, naso e gola.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 110m), Tar(Regime.Assicurativo, 100m));
        var audiometria = Pre("Esame audiometrico", "Valutazione strumentale della capacità uditiva.", 30,
            Tar(Regime.Ssn, 20m), Tar(Regime.Privato, 70m));
        var otorino = Srv("Otorinolaringoiatria", "Diagnosi e cura delle patologie di orecchio, naso e gola.",
            visitaOtorino, audiometria);

        var visitaOculistica = Pre("Visita oculistica", "Valutazione della vista e della salute oculare.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 110m), Tar(Regime.Assicurativo, 100m));
        var campoVisivo = Pre("Esame del campo visivo computerizzato", "Misurazione dell'ampiezza del campo visivo.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 80m));
        var oculistica = Srv("Oculistica", "Prevenzione, diagnosi e cura delle patologie dell'occhio.",
            visitaOculistica, campoVisivo);

        var visitaEndocrinologica = Pre("Visita endocrinologica", "Valutazione del sistema endocrino e del metabolismo.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 120m), Tar(Regime.Assicurativo, 110m));
        var visitaDiabetologica = Pre("Visita diabetologica", "Inquadramento e monitoraggio del paziente diabetico.", 30,
            Tar(Regime.Ssn, 25m), Tar(Regime.Privato, 110m));
        var endocrinologia = Srv("Endocrinologia", "Diagnosi e cura dei disturbi ormonali e metabolici.",
            visitaEndocrinologica, visitaDiabetologica);

        db.Servizi.AddRange(cardiologia, dermatologia, ortopedia, ginecologia, otorino, oculistica, endocrinologia);
        await db.SaveChangesAsync();

        // --- Medici (utente Identity + ruolo + entità di dominio), uno per servizio ---

        async Task<Medico> CreaMedicoAsync(string nome, string cognome, string email, string specializzazione, Servizio servizio)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nome = nome,
                Cognome = cognome
            };

            var createResult = await userManager.CreateAsync(user, PasswordMedici);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Creazione medico demo non riuscita ({email}): {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, AppRoles.Medico);

            var medico = new Medico
            {
                UserId = user.Id,
                Specializzazione = specializzazione,
                ServizioId = servizio.ServizioId
            };
            db.Medici.Add(medico);
            await db.SaveChangesAsync();
            return medico;
        }

        var medicoCardiologo = await CreaMedicoAsync("Laura", "Bianchi", "laura.bianchi@medicore.local", "Cardiologia", cardiologia);
        var medicoDermatologo = await CreaMedicoAsync("Marco", "Ferrari", "marco.ferrari@medicore.local", "Dermatologia", dermatologia);
        var medicoOrtopedico = await CreaMedicoAsync("Giulia", "Romano", "giulia.romano@medicore.local", "Ortopedia e traumatologia", ortopedia);
        var medicoGinecologo = await CreaMedicoAsync("Andrea", "Conti", "andrea.conti@medicore.local", "Ginecologia e ostetricia", ginecologia);
        var medicoOtorino = await CreaMedicoAsync("Francesca", "Greco", "francesca.greco@medicore.local", "Otorinolaringoiatria", otorino);

        // --- Turni settimanali ricorrenti (feriali, fascia 08:00-20:00, senza sovrapposizioni) ---

        Turno NuovoTurno(Medico medico, Prestazione prestazione, GiornoSettimana giorno, int oraInizio, int oraFine, int durataSlot) => new()
        {
            MedicoId = medico.MedicoId,
            PrestazioneId = prestazione.PrestazioneId,
            GiornoSettimana = giorno,
            OraInizio = new TimeOnly(oraInizio, 0),
            OraFine = new TimeOnly(oraFine, 0),
            DurataSlotMin = durataSlot
        };

        db.Turni.AddRange(
            NuovoTurno(medicoCardiologo, visitaCardiologica, GiornoSettimana.Lunedi, 9, 13, 30),
            NuovoTurno(medicoCardiologo, ecg, GiornoSettimana.Mercoledi, 14, 17, 20),
            NuovoTurno(medicoDermatologo, visitaDermatologica, GiornoSettimana.Martedi, 9, 12, 30),
            NuovoTurno(medicoDermatologo, mappaturaNei, GiornoSettimana.Giovedi, 15, 18, 30),
            NuovoTurno(medicoOrtopedico, visitaOrtopedica, GiornoSettimana.Lunedi, 14, 18, 30),
            NuovoTurno(medicoOrtopedico, infiltrazione, GiornoSettimana.Venerdi, 9, 11, 20),
            NuovoTurno(medicoGinecologo, visitaGinecologica, GiornoSettimana.Mercoledi, 9, 13, 30),
            NuovoTurno(medicoOtorino, visitaOtorino, GiornoSettimana.Giovedi, 9, 12, 30),
            NuovoTurno(medicoOtorino, audiometria, GiornoSettimana.Venerdi, 14, 17, 30));
        await db.SaveChangesAsync();

        // --- Pazienti (utente Identity + ruolo + entità di dominio) ---

        async Task CreaPazienteAsync(string nome, string cognome, string email, string codiceFiscale, DateOnly dataNascita, string telefono)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nome = nome,
                Cognome = cognome
            };

            var createResult = await userManager.CreateAsync(user, PasswordPazienti);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Creazione paziente demo non riuscita ({email}): {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, AppRoles.Paziente);

            db.Pazienti.Add(new Paziente
            {
                UserId = user.Id,
                CodiceFiscale = codiceFiscale,
                DataNascita = dataNascita,
                Telefono = telefono
            });
            await db.SaveChangesAsync();
        }

        await CreaPazienteAsync("Giovanni", "Esposito", "giovanni.esposito@example.com", "SPSGNN85M01H501Z", new DateOnly(1985, 8, 1), "3401234567");
        await CreaPazienteAsync("Anna", "Ricci", "anna.ricci@example.com", "RCCNNA90A41F205X", new DateOnly(1990, 1, 1), "3402345678");
        await CreaPazienteAsync("Luca", "Marino", "luca.marino@example.com", "MRNLCU78D12L219Q", new DateOnly(1978, 4, 12), "3403456789");
        await CreaPazienteAsync("Sofia", "Costa", "sofia.costa@example.com", "CSTSFO95E55H501W", new DateOnly(1995, 5, 15), "3404567890");
    }
}
