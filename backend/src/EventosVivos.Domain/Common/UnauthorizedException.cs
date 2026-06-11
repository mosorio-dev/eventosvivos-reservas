namespace EventosVivos.Domain.Common;

/// <summary>Thrown when credentials are invalid (mapped to HTTP 401 by the API).</summary>
public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
