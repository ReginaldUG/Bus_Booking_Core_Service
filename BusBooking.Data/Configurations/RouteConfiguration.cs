using BusBooking.Core.Constants;
using BusBooking.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");
        builder.HasKey(r => r.Id);

        //RELATIONSHIPS

        //INDEXES
        builder.HasIndex(r => r.RouteName).IsUnique();

        //CONSTRAINTS
        builder.ToTable(t => t.HasCheckConstraint(
            "chk_Route_Price", $"\"Price\" >= {Rules.MIN_ROUTE_PRICE}"
                ));
    }
}