using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Events.Dtos;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Events.Queries.GetEvent;

public sealed class GetEventQueryHandler : IRequestHandler<GetEventQuery, EventDto>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetEventQueryHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<EventDto> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Events
            .Include(e => e.Venue)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Evento", request.Id);

        return entity.ToDto(_clock.UtcNow);
    }
}
