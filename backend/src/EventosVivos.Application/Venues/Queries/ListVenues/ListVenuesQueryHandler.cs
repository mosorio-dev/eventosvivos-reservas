using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Venues.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Venues.Queries.ListVenues;

public sealed class ListVenuesQueryHandler : IRequestHandler<ListVenuesQuery, IReadOnlyList<VenueDto>>
{
    private readonly IAppDbContext _db;

    public ListVenuesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<VenueDto>> Handle(ListVenuesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Venues.AsNoTracking()
            .OrderBy(v => v.Id)
            .Select(v => new VenueDto(v.Id, v.Name, v.Capacity, v.City))
            .ToListAsync(cancellationToken);
    }
}
