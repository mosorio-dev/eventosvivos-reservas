using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Entities;

/// <summary>
/// Aggregate root for a cultural event. Encapsulates the invariants that do not
/// depend on other aggregates (RN03, length/positivity rules). Cross-aggregate
/// rules (RN01 venue capacity, RN02 venue overlap) are enforced by the create
/// handler, which has access to the venue and sibling events.
/// </summary>
public class Event : Entity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public int Capacity { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public decimal Price { get; private set; }
    public EventType Type { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Event() { } // EF Core

    public static Event Create(
        string title,
        string description,
        Venue venue,
        int capacity,
        DateTime startUtc,
        DateTime endUtc,
        decimal price,
        EventType type,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(venue);

        title = (title ?? string.Empty).Trim();
        description = (description ?? string.Empty).Trim();

        if (title.Length is < 5 or > 100)
            throw new BusinessRuleViolationException("EVENT_TITLE_LENGTH", "El título debe tener entre 5 y 100 caracteres.");

        if (description.Length is < 10 or > 500)
            throw new BusinessRuleViolationException("EVENT_DESCRIPTION_LENGTH", "La descripción debe tener entre 10 y 500 caracteres.");

        if (capacity <= 0)
            throw new BusinessRuleViolationException("EVENT_CAPACITY_POSITIVE", "La capacidad máxima debe ser un entero positivo.");

        // RN01: an event cannot exceed the capacity of its assigned venue.
        if (capacity > venue.Capacity)
            throw new BusinessRuleViolationException("EVENT_CAPACITY_EXCEEDS_VENUE", $"La capacidad ({capacity}) no puede superar la del venue ({venue.Capacity}).");

        if (startUtc <= nowUtc)
            throw new BusinessRuleViolationException("EVENT_START_IN_PAST", "La fecha de inicio debe ser futura.");

        if (endUtc <= startUtc)
            throw new BusinessRuleViolationException("EVENT_END_BEFORE_START", "La fecha de fin debe ser posterior al inicio.");

        if (price <= 0m)
            throw new BusinessRuleViolationException("EVENT_PRICE_POSITIVE", "El precio de entrada debe ser un decimal positivo.");

        // RN03: weekend events cannot start after 22:00.
        var isWeekend = startUtc.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        if (isWeekend && startUtc.TimeOfDay > new TimeSpan(22, 0, 0))
            throw new BusinessRuleViolationException("EVENT_WEEKEND_NIGHT", "Los eventos de fin de semana no pueden iniciar después de las 22:00.");

        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            VenueId = venue.Id,
            Capacity = capacity,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Price = price,
            Type = type,
            Status = EventStatus.Activo,
            CreatedAtUtc = nowUtc
        };
    }

    /// <summary>RN06: an event is considered completed once the current time passes its end.</summary>
    public EventStatus GetEffectiveStatus(DateTime nowUtc)
    {
        if (Status == EventStatus.Cancelado) return EventStatus.Cancelado;
        return nowUtc > EndUtc ? EventStatus.Completado : EventStatus.Activo;
    }

    public bool IsOpenForReservations(DateTime nowUtc) => GetEffectiveStatus(nowUtc) == EventStatus.Activo;

    public void Cancel() => Status = EventStatus.Cancelado;
}
