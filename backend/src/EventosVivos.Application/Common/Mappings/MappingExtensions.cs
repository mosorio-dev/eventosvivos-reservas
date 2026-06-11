using EventosVivos.Application.Events.Dtos;
using EventosVivos.Application.Reservations.Dtos;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Mappings;

/// <summary>
/// Explicit, allocation-friendly projections. Preferred over a convention-based
/// mapper here so the transformation (including effective status) is obvious and testable.
/// </summary>
public static class MappingExtensions
{
    public static EventDto ToDto(this Event e, DateTime nowUtc) => new(
        e.Id,
        e.Title,
        e.Description,
        e.VenueId,
        e.Venue?.Name,
        e.Capacity,
        e.StartUtc,
        e.EndUtc,
        e.Price,
        e.Type,
        e.GetEffectiveStatus(nowUtc),
        e.CreatedAtUtc);

    public static EventSummaryDto ToSummary(this Event e, DateTime nowUtc) => new(
        e.Id,
        e.Title,
        e.VenueId,
        e.Venue?.Name,
        e.Capacity,
        e.StartUtc,
        e.EndUtc,
        e.Price,
        e.Type,
        e.GetEffectiveStatus(nowUtc));

    public static ReservationDto ToDto(this Reservation r) => new(
        r.Id,
        r.EventId,
        r.Quantity,
        r.BuyerName,
        r.BuyerEmail.Value,
        r.Status,
        r.ReservationCode,
        r.CreatedAtUtc,
        r.ConfirmedAtUtc,
        r.CancelledAtUtc);
}
