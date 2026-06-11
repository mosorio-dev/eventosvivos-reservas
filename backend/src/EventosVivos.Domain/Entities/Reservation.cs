using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Entities;

/// <summary>
/// A purchase intent for one or more tickets of an event. Models the full state
/// machine: PendientePago -> Confirmada -> (Cancelada | Perdida).
/// </summary>
public class Reservation : Entity
{
    public Guid EventId { get; private set; }
    public Event? Event { get; private set; }
    public int Quantity { get; private set; }
    public string BuyerName { get; private set; } = null!;
    public Email BuyerEmail { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public string? ReservationCode { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    private Reservation() { } // EF Core

    public static Reservation Create(Guid eventId, int quantity, string buyerName, Email buyerEmail, DateTime nowUtc)
    {
        if (quantity < 1)
            throw new BusinessRuleViolationException("RESERVATION_QUANTITY_MIN", "La cantidad debe ser 1 o más.");

        buyerName = (buyerName ?? string.Empty).Trim();
        if (buyerName.Length == 0)
            throw new BusinessRuleViolationException("RESERVATION_BUYER_REQUIRED", "El nombre del comprador es obligatorio.");

        return new Reservation
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Quantity = quantity,
            BuyerName = buyerName,
            BuyerEmail = buyerEmail,
            Status = ReservationStatus.PendientePago,
            CreatedAtUtc = nowUtc
        };
    }

    /// <summary>RF-04: confirm payment, transitioning to Confirmada with a unique code.</summary>
    public void ConfirmPayment(string reservationCode, DateTime nowUtc)
    {
        switch (Status)
        {
            case ReservationStatus.Confirmada:
                throw new BusinessRuleViolationException("RESERVATION_ALREADY_CONFIRMED", "La reserva ya está confirmada.");
            case ReservationStatus.Cancelada:
            case ReservationStatus.Perdida:
                throw new BusinessRuleViolationException("RESERVATION_NOT_PAYABLE", "Una reserva cancelada no puede confirmarse.");
        }

        Status = ReservationStatus.Confirmada;
        ReservationCode = reservationCode;
        ConfirmedAtUtc = nowUtc;
    }

    /// <summary>
    /// RF-05 / RN07: cancel a reservation. A confirmed reservation cancelled less
    /// than 48h before the event is recorded as "Perdida" and its tickets are NOT
    /// released. Any other cancellable reservation releases its tickets.
    /// </summary>
    public void Cancel(DateTime eventStartUtc, DateTime nowUtc)
    {
        if (Status is ReservationStatus.Cancelada or ReservationStatus.Perdida)
            throw new BusinessRuleViolationException("RESERVATION_ALREADY_CLOSED", "La reserva ya fue cancelada.");

        var withinPenaltyWindow = eventStartUtc - nowUtc < TimeSpan.FromHours(48);

        Status = Status == ReservationStatus.Confirmada && withinPenaltyWindow
            ? ReservationStatus.Perdida
            : ReservationStatus.Cancelada;

        CancelledAtUtc = nowUtc;
    }
}
