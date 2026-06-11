using EventosVivos.Application.Venues.Dtos;
using MediatR;

namespace EventosVivos.Application.Venues.Queries.ListVenues;

public sealed record ListVenuesQuery : IRequest<IReadOnlyList<VenueDto>>;
