using BusBooking.Data.Commands.Implementations;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Executers.Implementations;
using BusBooking.Data.Executers.Interfaces;
using BusBooking.Data.Helpers.Implementations;
using BusBooking.Data.Helpers.Interfaces;
using BusBooking.Data.Queries.Implementations;
using BusBooking.Data.Queries.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusBooking.Data;

public static class BusBookingDataInjection
{
    public static void AddDataInjections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AppDbContext>();
        services.AddScoped<IReadExecuter, ReadExecuter>();
        services.AddScoped<IWriteExecuter, WriteExecuter>();
        services.AddScoped<IReadUtilities, ReadUtilities>();
        services.AddScoped<IWriteUtilities, WriteUtilities>();
        services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
    }
}