namespace EventosVivos.Domain.Common;

/// <summary>
/// Thrown when a business rule (RN01-RN07) or a functional invariant is violated.
/// Carries a machine-readable <see cref="Code"/> so the API exposes a stable error contract.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public string Code { get; }

    public BusinessRuleViolationException(string code, string message) : base(message)
    {
        Code = code;
    }
}
