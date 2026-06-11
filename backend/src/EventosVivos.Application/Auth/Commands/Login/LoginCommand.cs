using EventosVivos.Application.Auth.Dtos;
using MediatR;

namespace EventosVivos.Application.Auth.Commands.Login;

/// <summary>Authenticates an administrator and returns a JWT.</summary>
public sealed record LoginCommand(string Username, string Password) : IRequest<AuthResultDto>;
