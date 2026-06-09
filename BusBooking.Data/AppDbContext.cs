using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Bus> Buses { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<CustomerWallet> CustomerWallets { get; set; } = null!;
        public DbSet<CustomerWalletTransactions> CustomerWalletTransactions { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //RELATIONSHIPS
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Bus)
                .WithOne(b => b.Driver)
                .HasForeignKey<Driver>(d => d.BusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerWallet>()
                .HasOne(w => w.Customer)
                .WithOne(c => c.Wallet)
                .HasForeignKey<CustomerWallet>(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<CustomerWalletTransactions>()
                .HasOne(t => t.CustomerWallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.CustomerWalletId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Bus>()
                .HasOne(b=>b.Route)
                .WithOne(r=>r.Bus)
                .HasForeignKey<Bus>(b=>b.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Booking>()
                .HasOne(b=>b.Customer)
                .WithMany(c=>c.Bookings)
                .HasForeignKey(b=>b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Booking>()
                .HasOne(booking=>booking.Bus)
                .WithMany(bus=>bus.Bookings)
                .HasForeignKey(booking=>booking.BusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Booking>()
                .HasOne(b=>b.Route)
                .WithMany(r=>r.Bookings)
                .HasForeignKey(b=>b.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            //INDEXES
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.RouteId);
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.CustomerId);
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BusId);

            //ONE BUS PER ROUTE RULE
            modelBuilder.Entity<Bus>()
                .HasIndex(b => b.RouteId)
                .IsUnique();
            
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if(entityEntry.Property("UpdatedAt").CurrentValue != null)
                {
                    entityEntry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                if(entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
