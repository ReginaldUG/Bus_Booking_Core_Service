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
        public DbSet<Token> Tokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
