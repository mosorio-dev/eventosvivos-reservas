using EventosVivos.Api.Contracts;
using EventosVivos.Application.Reservations.Commands.CancelReservation;
using EventosVivos.Application.Reservations.Commands.ConfirmReservationPayment;
using EventosVivos.Application.Reservations.Commands.CreateReservation;
using EventosVivos.Application.Reservations.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ISender _mediator;

    public ReservationsController(ISender mediator) => _mediator = mediator;

    /// <summary>RF-03: reserve tickets (public — external user).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(request.EventId, request.Quantity, request.BuyerName, request.BuyerEmail);
        var dto = await _mediator.Send(command, cancellationToken);
        return Created($"/api/reservations/{dto.Id}", dto);
    }

    /// <summary>RF-04: confirm payment (administrator only).</summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Confirm(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ConfirmReservationPaymentCommand(id), cancellationToken));

    /// <summary>RF-05: cancel a reservation (public — "cualquier parte").</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CancelReservationCommand(id), cancellationToken));
}
