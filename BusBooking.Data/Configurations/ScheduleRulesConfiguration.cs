using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class ScheduleRulesConfiguration : IEntityTypeConfiguration<ScheduleRules>
{
    public void Configure(EntityTypeBuilder<ScheduleRules> builder)
    {
        builder.ToTable("ScheduleRules");
        builder.HasKey(sr => sr.Id);

        //RELATIONSHIPS
        builder.Property(sr => sr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sr => sr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(sr => sr.Route)
            .WithMany(r => r.ScheduleRules)
            .HasForeignKey(sr => sr.RouteId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //INDEXES
        builder.HasIndex(sr => sr.DayOfWeek);

        //CONSTRAINTS
    }
}