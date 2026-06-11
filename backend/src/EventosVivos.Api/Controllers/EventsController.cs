using EventosVivos.Api.Contracts;
using EventosVivos.Application.Events.Commands.CreateEvent;
using EventosVivos.Application.Events.Dtos;
using EventosVivos.Application.Events.Queries.GetEvent;
using EventosVivos.Application.Events.Queries.ListEvents;
using EventosVivos.Application.Reports.Dtos;
using EventosVivos.Application.Reports.Queries.GetOccupancyReport;
using EventosVivos.Application.Reservations.Dtos;
using EventosVivos.Application.Reservations.Queries.ListReservationsByEvent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public sealed class EventsController : ControllerBase
{
    private readonly ISender _mediator;

    public EventsController(ISender mediator) => _mediator = mediator;

    /// <summary>RF-01: create an event (administrator only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventDto>> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEventCommand(
            request.Title,
            request.Description,
            request.VenueId,
            request.Capacity,
            request.StartUtc,
            request.EndUtc,
            request.Price,
            request.Type);

        var dto = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>RF-02: list events with optional filters (public).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventSummaryDto>>> List(
        [FromQuery] ListEventsQuery query,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    /// <summary>Event detail (public).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetEventQuery(id), cancellationToken));

    /// <summary>RF-06: occupancy report (administrator only).</summary>
    [HttpGet("{id:guid}/report")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OccupancyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OccupancyReportDto>> Report(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOccupancyReportQuery(id), cancellationToken));

    /// <summary>Reservations of an event (administrator only).</summary>
    [HttpGet("{id:guid}/reservations")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> Reservations(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListReservationsByEventQuery(id), cancellationToken));
}
