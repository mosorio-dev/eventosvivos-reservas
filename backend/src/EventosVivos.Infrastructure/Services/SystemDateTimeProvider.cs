using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
