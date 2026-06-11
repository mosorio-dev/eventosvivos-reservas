using EventosVivos.Domain.Entities;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.BuyerName).IsRequired().HasMaxLength(150);

        builder.Property(r => r.BuyerEmail)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("BuyerEmail")
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ReservationCode).HasMaxLength(20);

        builder.HasIndex(r => r.EventId);
        builder.HasIndex(r => r.ReservationCode).IsUnique()
            .HasFilter("\"ReservationCode\" IS NOT NULL");
    }
}
