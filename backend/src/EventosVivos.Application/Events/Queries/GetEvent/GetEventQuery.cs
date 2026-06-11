using EventosVivos.Application.Events.Dtos;
using MediatR;

namespace EventosVivos.Application.Events.Queries.GetEvent;

public sealed record GetEventQuery(Guid Id) : IRequest<EventDto>;
