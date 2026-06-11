using EventosVivos.Domain.Enums;

namespace EventosVivos.Api.Contracts;

/// <summary>HTTP payload for RF-01. Decouples the public API from the application command.</summary>
public sealed record CreateEventRequest(
    string Title,
    string Description,
    int VenueId,
    int Capacity,
    DateTime StartUtc,
    DateTime EndUtc,
    decimal Price,
    EventType Type);
