using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class CustomerWalletTransactionConfiguration : IEntityTypeConfiguration<CustomerWalletTransactions>
{
    public void Configure(EntityTypeBuilder<CustomerWalletTransactions> builder)
    {
        builder.ToTable("CustomerWalletTransactions");
        builder.HasKey(tw => tw.Id);

        //RELATIONSHIPS
        builder.HasOne(t => t.CustomerWallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.CustomerWalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}