using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController : Controller
{
    private readonly ICustomerAuthenticationService _customerAuthenticationService;
    
    public CustomerController(ICustomerAuthenticationService customerAuthenticationService)
    {
        _customerAuthenticationService = customerAuthenticationService;
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
    
    [HttpPost("edit_customer")]
    public async Task<IActionResult> EditCustomer([FromBody] EditCustomerDetailsRequestDTO request)
    {
        var response = await _customerAuthenticationService.EditCustomerInformation(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }
}