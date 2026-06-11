using FluentValidation.Results;

namespace EventosVivos.Application.Common.Exceptions;

/// <summary>Aggregates FluentValidation failures into a single, API-friendly error.</summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Se encontraron uno o más errores de validación.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
