using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Mappings;
using EventosVivos.Application.Reservations.Dtos;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Reservations.Commands.ConfirmReservationPayment;

public sealed class ConfirmReservationPaymentCommandHandler : IRequestHandler<ConfirmReservationPaymentCommand, ReservationDto>
{
    private const int MaxCodeGenerationAttempts = 12;

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly IReservationCodeGenerator _codeGenerator;

    public ConfirmReservationPaymentCommandHandler(
        IAppDbContext db,
        IDateTimeProvider clock,
        IReservationCodeGenerator codeGenerator)
    {
        _db = db;
        _clock = clock;
        _codeGenerator = codeGenerator;
    }

    public async Task<ReservationDto> Handle(ConfirmReservationPaymentCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Reserva", request.ReservationId);

        var code = await GenerateUniqueCodeAsync(cancellationToken);
        reservation.ConfirmPayment(code, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return reservation.ToDto();
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var candidate = _codeGenerator.Next();
            var taken = await _db.Reservations.AnyAsync(r => r.ReservationCode == candidate, cancellationToken);
            if (!taken)
                return candidate;
        }

        throw new BusinessRuleViolationException(
            "RESERVATION_CODE_GENERATION_FAILED",
            "No se pudo generar un código de reserva único. Intente nuevamente.");
    }
}
