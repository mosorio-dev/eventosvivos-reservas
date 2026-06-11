using EventosVivos.Domain.Entities;

namespace EventosVivos.Domain.UnitTests;

internal static class TestData
{
    // 2026-01-01 is a Thursday; used as a stable "now" for deterministic rules.
    public static readonly DateTime Now = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

    public static Venue AuditorioCentral() => new(1, "Auditorio Central", 200, "Bogotá");
    public static Venue SalaNorte() => new(2, "Sala Norte", 50, "Bogotá");
}
