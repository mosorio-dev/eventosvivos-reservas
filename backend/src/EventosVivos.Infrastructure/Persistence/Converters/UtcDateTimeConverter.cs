using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventosVivos.Infrastructure.Persistence.Converters;

/// <summary>
/// SQLite stores datetimes as text and loses the <see cref="DateTimeKind"/>.
/// This converter persists values as UTC and re-stamps them as UTC on read, so
/// time-based business rules keep working with a consistent kind.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
