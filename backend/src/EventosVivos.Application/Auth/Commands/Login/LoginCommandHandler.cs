using EventosVivos.Application.Auth.Dtos;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Common;
using MediatR;

namespace EventosVivos.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private const string AdminRole = "Admin";

    private readonly IAdminAuthenticator _authenticator;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(IAdminAuthenticator authenticator, IJwtTokenGenerator tokenGenerator)
    {
        _authenticator = authenticator;
        _tokenGenerator = tokenGenerator;
    }

    public Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!_authenticator.ValidateCredentials(request.Username, request.Password))
            throw new UnauthorizedException("Usuario o contraseña inválidos.");

        var (token, expiresAtUtc) = _tokenGenerator.Generate(request.Username, AdminRole);
        return Task.FromResult(new AuthResultDto(token, expiresAtUtc, request.Username, AdminRole));
    }
}
