namespace EventosVivos.Application.Auth.Dtos;

public sealed record AuthResultDto(string Token, DateTime ExpiresAtUtc, string Username, string Role);
