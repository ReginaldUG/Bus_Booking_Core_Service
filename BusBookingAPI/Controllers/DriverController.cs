using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("driver")]
public class DriverController : Controller
{
    private readonly IDriverAuthenticationService _driverAuthenticationService;
    
    public DriverController(IDriverAuthenticationService driverAuthenticationService)
    {
        _driverAuthenticationService = driverAuthenticationService;
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