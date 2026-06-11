namespace EventosVivos.Domain.Enums;

public enum ReservationStatus
{
    /// <summary>Created, awaiting payment. Holds tickets.</summary>
    PendientePago = 1,

    /// <summary>Payment confirmed. Holds tickets and counts as a sale.</summary>
    Confirmada = 2,

    /// <summary>Cancelled. Tickets were released back to inventory.</summary>
    Cancelada = 3,

    /// <summary>
    /// RN07: a confirmed reservation cancelled with less than 48h to the event.
    /// Tickets are NOT released; counted only for reporting.
    /// </summary>
    Perdida = 4
}
