using EventosVivos.Application.Reservations.Dtos;
using MediatR;

namespace EventosVivos.Application.Reservations.Commands.ConfirmReservationPayment;

/// <summary>RF-04: confirm the payment of a reservation.</summary>
public sealed record ConfirmReservationPaymentCommand(Guid ReservationId) : IRequest<ReservationDto>;
