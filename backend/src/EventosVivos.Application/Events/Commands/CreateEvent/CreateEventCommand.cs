using EventosVivos.Application.Events.Dtos;
using EventosVivos.Domain.Enums;
using MediatR;

namespace EventosVivos.Application.Events.Commands.CreateEvent;

/// <summary>RF-01: create an event.</summary>
public sealed record CreateEventCommand(
    string Title,
    string Description,
    int VenueId,
    int Capacity,
    DateTime StartUtc,
    DateTime EndUtc,
    decimal Price,
    EventType Type) : IRequest<EventDto>;
