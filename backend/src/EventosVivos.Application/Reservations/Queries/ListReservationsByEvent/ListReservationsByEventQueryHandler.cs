using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Reservations.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Reservations.Queries.ListReservationsByEvent;

public sealed class ListReservationsByEventQueryHandler
    : IRequestHandler<ListReservationsByEventQuery, IReadOnlyList<ReservationDto>>
{
    private readonly IAppDbContext _db;

    public ListReservationsByEventQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReservationDto>> Handle(ListReservationsByEventQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _db.Reservations.AsNoTracking()
            .Where(r => r.EventId == request.EventId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return reservations.Select(r => r.ToDto()).ToList();
    }
}
