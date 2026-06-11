using System.Text.RegularExpressions;
using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.ValueObjects;

/// <summary>
/// Value object guaranteeing that a buyer email is structurally valid.
/// Acts as a last line of defense even if input validation is bypassed.
/// </summary>
public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();
        if (!EmailRegex().IsMatch(value))
        {
            throw new BusinessRuleViolationException(
                "RESERVATION_INVALID_EMAIL",
                $"El email '{raw}' no tiene un formato válido.");
        }

        return new Email(value.ToLowerInvariant());
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
