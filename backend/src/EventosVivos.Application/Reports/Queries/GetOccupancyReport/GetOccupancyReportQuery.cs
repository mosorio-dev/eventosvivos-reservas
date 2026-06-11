using EventosVivos.Application.Reports.Dtos;
using MediatR;

namespace EventosVivos.Application.Reports.Queries.GetOccupancyReport;

/// <summary>RF-06: occupancy report for a single event.</summary>
public sealed record GetOccupancyReportQuery(Guid EventId) : IRequest<OccupancyReportDto>;
