using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EventosVivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // "Sqlite" for zero-config local dev; "Postgres" for the deployed environment (e.g. Supabase).
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=eventosvivos.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(NormalizePostgresConnectionString(connectionString));
            else
                options.UseSqlite(connectionString);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IReservationCodeGenerator, ReservationCodeGenerator>();

        return services;
    }

    /// <summary>
    /// Accepts both the Npgsql keyword format and the "postgresql://user:pass@host:port/db" URI
    /// format that managed providers (Supabase, Render, Heroku, ...) hand out, so the connection
    /// string can be pasted as-is. Also enforces the SSL settings Supabase requires.
    /// </summary>
    private static string NormalizePostgresConnectionString(string raw)
    {
        if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return raw; // already in Npgsql keyword format
        }

        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = string.IsNullOrEmpty(database) ? "postgres" : database,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
