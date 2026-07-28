using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class RouteStopsConfiguration : IEntityTypeConfiguration<RouteStops>
{
    public void Configure(EntityTypeBuilder<RouteStops> builder)
    {
        builder.ToTable("RouteStops");
        builder.HasKey(rs => rs.Id);

        // Index
        builder.HasIndex(rs => new { rs.RouteId, rs.BusStopId }).IsUnique();

        //RELATIONSHIPS
        builder.HasOne(rs => rs.Route)
            .WithMany(r => r.RouteStops)
            .HasForeignKey(rs => rs.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.BusStops)
            .WithMany(bs => bs.RouteStops)
            .HasForeignKey(rs => rs.BusStopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}