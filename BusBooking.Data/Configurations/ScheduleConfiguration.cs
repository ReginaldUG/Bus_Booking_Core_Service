using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules");
        builder.HasKey(s => s.Id);

        //PROPERTY CONFIGURATION
        builder.Property(s => s.DepartureTime)
            .IsRequired();
        builder.Property(s => s.DateOfDeparture).IsRequired();

        builder.Property(s => s.AvailableSeats)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        //RELATIONSHIPS
        builder.HasOne(s => s.Route)
            .WithMany(r => r.Schedules)
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.Bus)
            .WithMany() 
            .HasForeignKey(s => s.BusId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(s => s.Bookings)
            .WithOne(b => b.Schedule)
            .HasForeignKey(b => b.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}