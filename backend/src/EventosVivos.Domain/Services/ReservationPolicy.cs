using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Services;

/// <summary>
/// Pure, table-driven reservation policies. Centralizes the transaction limits
/// (RF-03 imminent rule, RN05 price rule) and the late-reservation cutoff (RN04).
/// </summary>
public static class ReservationPolicy
{
    public const int MaxTicketsWhenImminent = 5;   // RF-03: < 24h to start
    public const int MaxTicketsWhenExpensive = 10;  // RN05: price > $100
    public const decimal ExpensivePriceThreshold = 100m;

    public static readonly TimeSpan LateReservationCutoff = TimeSpan.FromHours(1);   // RN04
    public static readonly TimeSpan ImminentWindow = TimeSpan.FromHours(24);          // RF-03

    /// <summary>RN04: no reservations for events starting in less than one hour.</summary>
    public static void EnsureReservationWindowOpen(DateTime eventStartUtc, DateTime nowUtc)
    {
        if (eventStartUtc - nowUtc < LateReservationCutoff)
        {
            throw new BusinessRuleViolationException(
                "RESERVATION_WINDOW_CLOSED",
                "No se permiten reservas para eventos que inicien en menos de 1 hora.");
        }
    }

    /// <summary>
    /// Maximum tickets allowed in a single transaction given the active rules.
    /// When several limits apply, the strictest wins.
    /// </summary>
    public static int MaxTicketsPerTransaction(decimal price, DateTime eventStartUtc, DateTime nowUtc)
    {
        var max = int.MaxValue;

        if (price > ExpensivePriceThreshold)
            max = Math.Min(max, MaxTicketsWhenExpensive);

        if (eventStartUtc - nowUtc < ImminentWindow)
            max = Math.Min(max, MaxTicketsWhenImminent);

        return max;
    }

    public static void EnsureQuantityWithinTransactionLimit(int quantity, decimal price, DateTime eventStartUtc, DateTime nowUtc)
    {
        var max = MaxTicketsPerTransaction(price, eventStartUtc, nowUtc);
        if (quantity > max)
        {
            throw new BusinessRuleViolationException(
                "RESERVATION_QUANTITY_LIMIT",
                $"Esta transacción permite un máximo de {max} entradas.");
        }
    }
}
