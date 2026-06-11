using EventosVivos.Application.Reservations.Dtos;
using MediatR;

namespace EventosVivos.Application.Reservations.Commands.CreateReservation;

/// <summary>RF-03: reserve tickets for an event.</summary>
public sealed record CreateReservationCommand(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail) : IRequest<ReservationDto>;
