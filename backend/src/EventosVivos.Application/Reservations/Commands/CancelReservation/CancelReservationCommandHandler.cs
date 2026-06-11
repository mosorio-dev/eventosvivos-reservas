using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Reservations.Dtos;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Reservations.Commands.CancelReservation;

public sealed class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, ReservationDto>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CancelReservationCommandHandler(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Reserva", request.ReservationId);

        if (reservation.Event is null)
            throw new NotFoundException("Evento", reservation.EventId);

        reservation.Cancel(reservation.Event.StartUtc, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return reservation.ToDto();
    }
}
