using EventosVivos.Application.Auth.Commands.Login;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Common;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Application.IntegrationTests;

public class AuthCommandTests
{
    private sealed class StubAdminAuthenticator : IAdminAuthenticator
    {
        private readonly bool _result;
        public StubAdminAuthenticator(bool result) => _result = result;
        public bool ValidateCredentials(string username, string password) => _result;
    }

    private sealed class StubJwtTokenGenerator : IJwtTokenGenerator
    {
        public (string Token, DateTime ExpiresAtUtc) Generate(string username, string role)
            => ("fake.jwt.token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAdminToken()
    {
        var handler = new LoginCommandHandler(new StubAdminAuthenticator(true), new StubJwtTokenGenerator());

        var result = await handler.Handle(new LoginCommand("admin", "secret"), CancellationToken.None);

        result.Token.Should().Be("fake.jwt.token");
        result.Role.Should().Be("Admin");
        result.Username.Should().Be("admin");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ThrowsUnauthorized()
    {
        var handler = new LoginCommandHandler(new StubAdminAuthenticator(false), new StubJwtTokenGenerator());

        var act = () => handler.Handle(new LoginCommand("admin", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
