using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("Tokens");
        builder.HasKey(t => t.Id);

        //RELATIONSHIPS
        builder.HasOne(t => t.Customer)
            .WithMany(c => c.Tokens)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        //INDEXES
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.TokenHash);

        //CONSTRAINTS
        builder.HasIndex(e => new { e.CustomerId, e.TokenHash })
            .IsUnique()
            .HasDatabaseName("uq_owner_token");
    }
}