using EventosVivos.Application.Reservations.Dtos;
using MediatR;

namespace EventosVivos.Application.Reservations.Commands.CancelReservation;

/// <summary>RF-05 / RN07: cancel a reservation.</summary>
public sealed record CancelReservationCommand(Guid ReservationId) : IRequest<ReservationDto>;
