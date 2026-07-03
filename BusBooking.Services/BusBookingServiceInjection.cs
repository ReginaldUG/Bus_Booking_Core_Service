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
        services.AddScoped<IDriverAuthenticationService, DriverAuthenticationService>();
        services.AddScoped<IAccountEvaluationService, AccountEvaluationService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<AuthenticationHelper>();
        services.AddScoped<GeneralHelpers>();
    }
}