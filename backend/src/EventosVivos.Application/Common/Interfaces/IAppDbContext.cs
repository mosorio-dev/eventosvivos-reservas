using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction the application layer depends on, keeping it ignorant
/// of the concrete EF Core provider (Dependency Inversion).
/// </summary>
public interface IAppDbContext
{
    DbSet<Event> Events { get; }
    DbSet<Venue> Venues { get; }
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
