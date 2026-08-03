using BusBooking.Services.BL.Implementations;
using BusBooking.Services.BL.Interfaces;
using BusBooking.Services.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusBooking.Services;

public static class BusBookingServiceInjection
{
    public static void AddServiceInjections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerAuthenticationService, CustomerAuthenticationService>();
        services.AddScoped<IDriverAuthenticationService, DriverAuthenticationService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthenticationHelper>();
        services.AddScoped<EmailHelper>();
        services.AddScoped<GeneralHelpers>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
    }
}