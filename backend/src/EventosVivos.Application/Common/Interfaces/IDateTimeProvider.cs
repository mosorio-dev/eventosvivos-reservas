namespace EventosVivos.Application.Common.Interfaces;

/// <summary>Abstracts the system clock so time-dependent rules are deterministically testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
