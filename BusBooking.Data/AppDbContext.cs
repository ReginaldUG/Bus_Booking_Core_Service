using BusBooking.Core.Constants;
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
        public DbSet<Schedule> Schedules { get; set; } = null!;
        public DbSet<ScheduleRules> ScheduleRules { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            
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

            modelBuilder.Entity<CustomerWalletTransactions>()
                .HasOne(t => t.CustomerWallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.CustomerWalletId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Booking>()
                .HasOne(b=>b.Customer)
                .WithMany(c=>c.Bookings)
                .HasForeignKey(b=>b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            
            //INDEXES
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.Email)
                .IsUnique();
            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Bus>()
                .HasIndex(b => b.PlateNumber)
                .IsUnique();

            modelBuilder.Entity<Route>()
                .HasIndex(r => r.RouteName)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.CustomerId);
            
            //CONSTRAINTS
            modelBuilder.Entity<Booking>()
                .ToTable(t => t.HasCheckConstraint(
                    "chk_CancelledBy", "\"CancelledBy\" IN ('customer', 'driver')"
                ));
            modelBuilder.Entity<Booking>()
                .ToTable(t => t.HasCheckConstraint(
                    "chk_CancelledBy_Condition", 
                    "(\"isCancelled\" = true AND \"CancelledBy\" IN ('customer', 'driver')) OR (\"isCancelled\" = false AND \"CancelledBy\" IS NULL)"
                ));

            modelBuilder.Entity<Route>()
                .ToTable(t => t.HasCheckConstraint(
                    "chk_Route_Price", $"\"Price\" >= {Rules.MIN_ROUTE_PRICE}"
                ));
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
