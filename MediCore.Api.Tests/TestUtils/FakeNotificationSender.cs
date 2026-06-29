using MediCore.Api.Domain.Entities;
using MediCore.Api.Services.Notifiche;

namespace MediCore.Api.Tests.TestUtils;

// Sender fittizio: registra le notifiche "inviate" senza alcun canale reale.
public class FakeNotificationSender : INotificationSender
{
    public List<Notifica> Inviate { get; } = new();

    public Task<bool> SendAsync(Notifica notifica, CancellationToken cancellationToken = default)
    {
        Inviate.Add(notifica);
        return Task.FromResult(true);
    }
}
