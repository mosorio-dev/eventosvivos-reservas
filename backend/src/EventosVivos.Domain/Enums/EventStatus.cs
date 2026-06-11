namespace EventosVivos.Domain.Enums;

public enum EventStatus
{
    /// <summary>Open and not yet finished.</summary>
    Activo = 1,

    /// <summary>Cancelled by an organizer.</summary>
    Cancelado = 2,

    /// <summary>Derived state (RN06): current time is past the event end.</summary>
    Completado = 3
}
