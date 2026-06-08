using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Implementations;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : Controller
{
    private readonly CustomerAuthenticationService _customerAuthenticationService;
    
    public AuthenticationController(CustomerAuthenticationService customerAuthenticationService)
    {
        _customerAuthenticationService = customerAuthenticationService;
    }

    [HttpPost("register_customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerRegisterTask(request);

        return HttpResponseHelper.GetHttpResponse(response);
    }

    
}


