namespace EventosVivos.Domain.Services;

/// <summary>Pure scheduling helpers (RN02).</summary>
public static class SchedulingPolicy
{
    /// <summary>
    /// Half-open interval overlap test: [startA, endA) intersects [startB, endB).
    /// Back-to-back events (one ending exactly when the next begins) do NOT overlap.
    /// </summary>
    public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        => startA < endB && startB < endA;
}
