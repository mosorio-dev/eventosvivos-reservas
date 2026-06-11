namespace EventosVivos.Domain.Enums;

public static class ReservationStatusExtensions
{
    /// <summary>
    /// Whether a reservation in this state still occupies inventory (cannot be resold).
    /// Cancelled reservations release their tickets; everything else holds them.
    /// </summary>
    public static bool OccupiesInventory(this ReservationStatus status) => status switch
    {
        ReservationStatus.PendientePago => true,
        ReservationStatus.Confirmada => true,
        ReservationStatus.Perdida => true,
        ReservationStatus.Cancelada => false,
        _ => false
    };
}
