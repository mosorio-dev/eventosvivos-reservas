using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Events.Dtos;

public sealed record EventDto(
    Guid Id,
    string Title,
    string Description,
    int VenueId,
    string? VenueName,
    int Capacity,
    DateTime StartUtc,
    DateTime EndUtc,
    decimal Price,
    EventType Type,
    EventStatus Status,
    DateTime CreatedAtUtc);
