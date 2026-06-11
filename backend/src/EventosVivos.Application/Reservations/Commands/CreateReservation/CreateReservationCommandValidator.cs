using FluentValidation;

namespace EventosVivos.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser 1 o más.");
        RuleFor(x => x.BuyerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BuyerEmail).NotEmpty().EmailAddress().WithMessage("El email no tiene un formato válido.");
    }
}
