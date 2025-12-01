using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.VIN)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(c => c.LicensePlate)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(c => c.PricePerDay)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.PricePerHour)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Description)
            .HasMaxLength(1000);
    }
}
