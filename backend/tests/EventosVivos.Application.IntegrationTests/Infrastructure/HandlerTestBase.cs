using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.IntegrationTests.Infrastructure;

/// <summary>
/// Spins up a real EF Core model on an in-memory SQLite connection so handlers are
/// exercised against actual SQL (conversions, GROUP BY, filters) — closer to production
/// than the EF in-memory provider.
/// </summary>
public abstract class HandlerTestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected AppDbContext Db { get; }
    protected FakeDateTimeProvider Clock { get; } = new();
    protected SequentialReservationCodeGenerator CodeGenerator { get; } = new();

    protected HandlerTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();

        Db.Venues.AddRange(
            new Venue(1, "Auditorio Central", 200, "Bogotá"),
            new Venue(2, "Sala Norte", 50, "Bogotá"),
            new Venue(3, "Arena Sur", 500, "Medellín"));
        Db.SaveChanges();
    }

    /// <summary>Persists an event directly through the aggregate factory (bypassing the handler).</summary>
    protected async Task<Event> GivenEventAsync(
        int venueId = 1,
        int capacity = 100,
        DateTime? start = null,
        DateTime? end = null,
        decimal price = 50m,
        EventType type = EventType.Conferencia)
    {
        var venue = await Db.Venues.FirstAsync(v => v.Id == venueId);
        var s = start ?? Clock.UtcNow.AddDays(10);
        var e = end ?? s.AddHours(2);
        var ev = Event.Create("Evento de prueba", "Descripción de prueba suficientemente larga.",
            venue, capacity, s, e, price, type, Clock.UtcNow);
        Db.Events.Add(ev);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        return ev;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
