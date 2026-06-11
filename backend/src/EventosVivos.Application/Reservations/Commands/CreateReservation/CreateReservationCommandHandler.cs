using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Reservations.Dtos;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Services;
using EventosVivos.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CreateReservationCommandHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException("Evento", request.EventId);

        if (!ev.IsOpenForReservations(now))
        {
            throw new BusinessRuleViolationException(
                "EVENT_NOT_OPEN",
                "El evento no admite reservas (cancelado o ya finalizado).");
        }

        // RN04: reservation window must still be open.
        ReservationPolicy.EnsureReservationWindowOpen(ev.StartUtc, now);

        // RF-03 (< 24h => max 5) and RN05 (price > $100 => max 10). Strictest wins.
        ReservationPolicy.EnsureQuantityWithinTransactionLimit(request.Quantity, ev.Price, ev.StartUtc, now);

        // Availability: only cancelled reservations free inventory.
        var held = await _db.Reservations
            .Where(r => r.EventId == request.EventId
                        && (r.Status == ReservationStatus.PendientePago
                            || r.Status == ReservationStatus.Confirmada
                            || r.Status == ReservationStatus.Perdida))
            .SumAsync(r => r.Quantity, cancellationToken);

        var available = ev.Capacity - held;
        if (request.Quantity > available)
        {
            throw new BusinessRuleViolationException(
                "EVENT_INSUFFICIENT_CAPACITY",
                $"No hay entradas suficientes. Disponibles: {Math.Max(0, available)}.");
        }

        var email = Email.Create(request.BuyerEmail);
        var reservation = Reservation.Create(request.EventId, request.Quantity, request.BuyerName, email, now);

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(cancellationToken);

        return reservation.ToDto();
    }
}
