using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Implementations;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : Controller
{
    private readonly ICustomerAuthenticationService _customerAuthenticationService;
    private readonly IDriverAuthenticationService _driverAuthenticationService;
    
    public AuthenticationController(ICustomerAuthenticationService customerAuthenticationService, IDriverAuthenticationService driverAuthenticationService)
    {
        _customerAuthenticationService = customerAuthenticationService;
        _driverAuthenticationService = driverAuthenticationService;

    }

    [HttpPost("register_customer")]
    public async Task<IActionResult> CustomerRegister([FromBody] CustomerRegisterRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerRegisterTask(request);

        return HttpResponseHelper.GetHttpResponse(response);
    }
    
    [HttpPost("login_customer")]
    public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerLoginTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("login_driver")]
    public async Task<IActionResult> DriverLogin([FromBody] DriverLoginRequestDTO request)
    {
        var response = await _driverAuthenticationService.DriverLoginTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("register_driver")]
    public async Task<IActionResult> DriverRegister([FromBody] DriverRegisterRequestDTO request)
    {
        var response = await _driverAuthenticationService.DriverRegisterTask(request);

        return HttpResponseHelper.GetHttpResponse(response);
    }

    
}


