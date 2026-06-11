using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Events.Dtos;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CreateEventCommandHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var venue = await _db.Venues.FirstOrDefaultAsync(v => v.Id == request.VenueId, cancellationToken)
            ?? throw new NotFoundException("Venue", request.VenueId);

        // RN02: an active event cannot overlap another active event at the same venue.
        var hasOverlap = await _db.Events.AnyAsync(
            e => e.VenueId == request.VenueId
                 && e.Status != EventStatus.Cancelado
                 && e.StartUtc < request.EndUtc
                 && request.StartUtc < e.EndUtc,
            cancellationToken);

        if (hasOverlap)
        {
            throw new BusinessRuleViolationException(
                "EVENT_VENUE_OVERLAP",
                "El venue ya tiene un evento activo en un horario que se superpone.");
        }

        // RN01, RN03 and remaining invariants are enforced inside the aggregate factory.
        var newEvent = Event.Create(
            request.Title,
            request.Description,
            venue,
            request.Capacity,
            request.StartUtc,
            request.EndUtc,
            request.Price,
            request.Type,
            _clock.UtcNow);

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync(cancellationToken);

        // Navigation property is not loaded after Add, so the venue name is set explicitly.
        return newEvent.ToDto(_clock.UtcNow) with { VenueName = venue.Name };
    }
}
