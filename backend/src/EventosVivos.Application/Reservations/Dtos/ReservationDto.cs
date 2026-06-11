using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Reservations.Dtos;

public sealed record ReservationDto(
    Guid Id,
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    ReservationStatus Status,
    string? ReservationCode,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CancelledAtUtc);
