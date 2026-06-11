using System.Data.Common;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventosVivos.Infrastructure.Persistence;

/// <summary>Ensures the schema exists and the reference venue catalog is present.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var creator = db.GetService<IRelationalDatabaseCreator>();

        // Create the database itself if it does not exist (e.g. a fresh local SQLite file).
        // Managed Postgres such as Supabase already has the "postgres" database, so this is a no-op there.
        if (!await creator.ExistsAsync(cancellationToken))
            await creator.CreateAsync(cancellationToken);

        // NOTE: HasTables()/EnsureCreated() are unreliable on managed Postgres because Supabase ships
        // system schemas (auth, storage, ...) — EF would think the schema already exists and skip our
        // tables. Instead we probe our OWN table and create the schema only when it is missing. This
        // works identically for a fresh SQLite file and an existing PostgreSQL database.
        if (!await OurSchemaExistsAsync(db, cancellationToken))
            await creator.CreateTablesAsync(cancellationToken);

        if (!await db.Venues.AnyAsync(cancellationToken))
        {
            db.Venues.AddRange(
                new Venue(1, "Auditorio Central", 200, "Bogotá"),
                new Venue(2, "Sala Norte", 50, "Bogotá"),
                new Venue(3, "Arena Sur", 500, "Medellín"));

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<bool> OurSchemaExistsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.Venues.AnyAsync(cancellationToken);
            return true;
        }
        catch (DbException)
        {
            // Table does not exist yet (42P01 on Postgres / "no such table" on SQLite).
            return false;
        }
    }
}
