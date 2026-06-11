using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventosVivos.Infrastructure.Persistence.Converters;

/// <summary>Nullable counterpart of <see cref="UtcDateTimeConverter"/>.</summary>
public sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        v => v.HasValue ? v.Value.ToUniversalTime() : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
