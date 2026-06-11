using EventosVivos.Application.Events.Dtos;
using EventosVivos.Domain.Enums;
using MediatR;

namespace EventosVivos.Application.Events.Queries.ListEvents;

/// <summary>RF-02: list events with optional filters.</summary>
public sealed record ListEventsQuery(
    EventType? Type = null,
    DateTime? StartFromUtc = null,
    DateTime? StartToUtc = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null) : IRequest<IReadOnlyList<EventSummaryDto>>;
