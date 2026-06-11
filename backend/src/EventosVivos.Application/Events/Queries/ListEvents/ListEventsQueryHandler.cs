using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Events.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Events.Queries.ListEvents;

public sealed class ListEventsQueryHandler : IRequestHandler<ListEventsQuery, IReadOnlyList<EventSummaryDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ListEventsQueryHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<EventSummaryDto>> Handle(ListEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Events.Include(e => e.Venue).AsNoTracking().AsQueryable();

        if (request.Type is not null)
            query = query.Where(e => e.Type == request.Type);

        if (request.VenueId is not null)
            query = query.Where(e => e.VenueId == request.VenueId);

        if (request.StartFromUtc is not null)
            query = query.Where(e => e.StartUtc >= request.StartFromUtc);

        if (request.StartToUtc is not null)
            query = query.Where(e => e.StartUtc <= request.StartToUtc);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var term = request.Title.Trim().ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(term));
        }

        var now = _clock.UtcNow;
        var events = await query.OrderBy(e => e.StartUtc).ToListAsync(cancellationToken);

        // Status is partly derived (RN06 "completado"), so the status filter is applied in memory.
        var projected = events.Select(e => e.ToSummary(now));

        if (request.Status is not null)
            projected = projected.Where(s => s.Status == request.Status);

        return projected.ToList();
    }
}
