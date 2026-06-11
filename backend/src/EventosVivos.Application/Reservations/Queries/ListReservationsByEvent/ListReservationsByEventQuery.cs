using EventosVivos.Application.Reservations.Dtos;
using MediatR;

namespace EventosVivos.Application.Reservations.Queries.ListReservationsByEvent;

public sealed record ListReservationsByEventQuery(Guid EventId) : IRequest<IReadOnlyList<ReservationDto>>;
