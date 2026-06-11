using EventosVivos.Application.Venues.Dtos;
using EventosVivos.Application.Venues.Queries.ListVenues;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[ApiController]
[Route("api/venues")]
[Produces("application/json")]
public sealed class VenuesController : ControllerBase
{
    private readonly ISender _mediator;

    public VenuesController(ISender mediator) => _mediator = mediator;

    /// <summary>Reference venue catalog (used by the event creation form).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VenueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VenueDto>>> List(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListVenuesQuery(), cancellationToken));
}
