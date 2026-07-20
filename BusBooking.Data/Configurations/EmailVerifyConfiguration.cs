using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;


public class EmailVerifyConfiguration : IEntityTypeConfiguration<EmailVerify>
{
    public void Configure(EntityTypeBuilder<EmailVerify> builder)
    {
        builder.ToTable("EmailVerify");
        builder.HasKey(e => e.Id);
    }
}