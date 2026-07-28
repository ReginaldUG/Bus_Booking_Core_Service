using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class BusStopConfiguration : IEntityTypeConfiguration<BusStops>
{
    public void Configure(EntityTypeBuilder<BusStops> builder)
    {
        builder.ToTable("BusStops");
        builder.HasIndex(bs => bs.Id);

        builder.Property(bs => bs.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(bs => bs.Name)
            .IsUnique();
    }
}