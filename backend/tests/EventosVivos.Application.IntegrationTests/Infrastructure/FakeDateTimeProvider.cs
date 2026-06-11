using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Application.IntegrationTests.Infrastructure;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
}
