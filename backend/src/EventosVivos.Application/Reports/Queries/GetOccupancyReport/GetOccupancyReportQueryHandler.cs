using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Reports.Dtos;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Reports.Queries.GetOccupancyReport;

public sealed class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetOccupancyReportQueryHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        var ev = await _db.Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException("Evento", request.EventId);

        var byStatus = await _db.Reservations.AsNoTracking()
            .Where(r => r.EventId == request.EventId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Quantity = g.Sum(r => r.Quantity) })
            .ToListAsync(cancellationToken);

        int QuantityFor(ReservationStatus status) =>
            byStatus.FirstOrDefault(x => x.Status == status)?.Quantity ?? 0;

        var sold = QuantityFor(ReservationStatus.Confirmada);
        var pending = QuantityFor(ReservationStatus.PendientePago);
        var lost = QuantityFor(ReservationStatus.Perdida);

        // Cancelled reservations release inventory; everything else occupies it.
        var held = sold + pending + lost;
        var available = Math.Max(0, ev.Capacity - held);
        var occupancy = ev.Capacity == 0 ? 0m : Math.Round((decimal)held / ev.Capacity * 100m, 2);

        // RF-06: revenue is defined as price x confirmed tickets.
        var revenue = ev.Price * sold;

        return new OccupancyReportDto(
            ev.Id,
            ev.Title,
            ev.Capacity,
            sold,
            pending,
            lost,
            available,
            occupancy,
            revenue,
            ev.GetEffectiveStatus(_clock.UtcNow));
    }
}
