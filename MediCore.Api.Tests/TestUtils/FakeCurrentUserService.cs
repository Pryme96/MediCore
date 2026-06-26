using MediCore.Api.Services;

namespace MediCore.Api.Tests.TestUtils;

public class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; } = "test-user";
    public string? UserName { get; set; } = "test-user@medicore.local";
}
