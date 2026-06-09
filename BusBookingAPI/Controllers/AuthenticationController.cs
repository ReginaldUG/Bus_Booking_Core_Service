using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Implementations;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : Controller
{
    private readonly ICustomerAuthenticationService _customerAuthenticationService;
    
    public AuthenticationController(ICustomerAuthenticationService customerAuthenticationService)
    {
        _customerAuthenticationService = customerAuthenticationService;
    }

    [HttpPost("register_customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerRegisterTask(request);

        return HttpResponseHelper.GetHttpResponse(response);
    }
    
    [HttpPost("login_customer")]
    public async Task<IActionResult> LoginCustomer([FromBody] CustomerLoginRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerLoginTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    
}


