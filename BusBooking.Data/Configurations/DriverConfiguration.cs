using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.Id);

        //RELATIONSHIPS
        builder.HasOne(d => d.Bus)
            .WithOne(b => b.Driver)
            .HasForeignKey<Driver>(d => d.BusId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //INDEXES
        builder.HasIndex(d => d.Email).IsUnique();
        builder.HasIndex(d => d.PhoneNumber).IsUnique();

        
    }
}