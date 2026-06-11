using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Reports.Dtos;

public sealed record OccupancyReportDto(
    Guid EventId,
    string EventTitle,
    int Capacity,
    int TicketsSold,
    int TicketsPending,
    int TicketsLost,
    int TicketsAvailable,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    EventStatus Status);
