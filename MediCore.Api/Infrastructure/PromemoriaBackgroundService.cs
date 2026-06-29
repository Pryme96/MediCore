using MediCore.Api.Services;

namespace MediCore.Api.Infrastructure;

// Worker in background: a intervalli regolari genera i promemoria per gli appuntamenti
// confermati in arrivo (la logica vera è in NotificaService.GeneraPromemoriaDovutiAsync,
// così resta unit-testabile). Apre uno scope a ogni giro perché NotificaService è scoped.
public class PromemoriaBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<PromemoriaBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _intervallo = TimeSpan.FromMinutes(
        config.GetValue<int?>("Notifiche:IntervalloScansioneMinuti") ?? 15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_intervallo);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var notifiche = scope.ServiceProvider.GetRequiredService<INotificaService>();
                var creati = await notifiche.GeneraPromemoriaDovutiAsync(stoppingToken);
                if (creati > 0)
                    logger.LogInformation("Generati {Conteggio} promemoria appuntamento.", creati);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Errore durante la generazione dei promemoria appuntamento.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
