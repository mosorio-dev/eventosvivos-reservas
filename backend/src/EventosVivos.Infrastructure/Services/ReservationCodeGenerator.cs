using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Infrastructure.Services;

/// <summary>Produces codes in the format EV-{6 digits}. Uniqueness is enforced by the handler.</summary>
public sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    public string Next() => $"EV-{Random.Shared.Next(0, 1_000_000):D6}";
}
