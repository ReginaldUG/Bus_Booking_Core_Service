using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class CustomerWalletConfiguration : IEntityTypeConfiguration<CustomerWallet>
{
    public void Configure(EntityTypeBuilder<CustomerWallet> builder)
    {
        builder.ToTable("CustomerWallets");
        builder.HasKey(w => w.Id);

        //RELATIONSHIPS
        builder.HasOne(w => w.Customer)
            .WithOne(c => c.Wallet)
            .HasForeignKey<CustomerWallet>(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        

    }
}