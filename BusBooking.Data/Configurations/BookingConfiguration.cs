using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        //RELATIONSHIPS
        builder.HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Schedule)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //INDEXES
        builder.HasIndex(b => b.ScheduleId);
        builder.HasIndex(b => b.CustomerId);

        //CONSTRAINTS
        builder.ToTable(t => t.HasCheckConstraint(
            "chk_CancelledBy", "\"CancelledBy\" IN ('customer', 'driver')"));
        builder.ToTable(t => t.HasCheckConstraint(
            "chk_CancelledBy_Condition",
            "(\"IsCancelled\" = true AND \"CancelledBy\" IN ('customer', 'driver')) OR (\"IsCancelled\" = false AND \"CancelledBy\" IS NULL)"));
    }
}