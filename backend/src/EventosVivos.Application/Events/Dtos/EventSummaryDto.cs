using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Events.Dtos;

public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    int VenueId,
    string? VenueName,
    int Capacity,
    DateTime StartUtc,
    DateTime EndUtc,
    decimal Price,
    EventType Type,
    EventStatus Status);
