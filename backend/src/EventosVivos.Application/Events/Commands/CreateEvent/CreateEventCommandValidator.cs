using EventosVivos.Application.Common.Interfaces;
using FluentValidation;

namespace EventosVivos.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(5).MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(500);
        RuleFor(x => x.VenueId).GreaterThan(0);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0m);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.StartUtc)
            .GreaterThan(clock.UtcNow).WithMessage("La fecha de inicio debe ser futura.");
        RuleFor(x => x.EndUtc)
            .GreaterThan(x => x.StartUtc).WithMessage("La fecha de fin debe ser posterior al inicio.");
    }
}
