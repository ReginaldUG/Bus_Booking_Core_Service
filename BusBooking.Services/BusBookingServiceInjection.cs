using BusBooking.Services.BL.Implementations;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusBooking.Services;

public static class BusBookingServiceInjection
{
    public static void AddServiceInjections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerAuthenticationService, CustomerAuthenticationService>();
        services.AddScoped<AuthenticationHelper>();
    }
}