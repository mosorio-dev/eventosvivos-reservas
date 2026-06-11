namespace EventosVivos.Domain.Common;

/// <summary>
/// Base type for every error that represents a violation of a business invariant.
/// The API layer translates these into 4xx responses instead of 500s.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
