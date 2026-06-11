namespace EventosVivos.Application.Common.Interfaces;

/// <summary>Issues signed JWT access tokens for an authenticated principal.</summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(string username, string role);
}
