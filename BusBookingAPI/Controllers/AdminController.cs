using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBooking.Services.Helpers;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ICustomerAuthenticationService _customerAuthenticationService;
    private readonly IScheduleService _scheduleService;
    private readonly EmailHelper _emailHelper;
    private readonly IBusService _busService;

    public AdminController (IAdminService adminService, IBusService busService, 
        EmailHelper emailHelper, IScheduleService scheduleService,
        ICustomerAuthenticationService customerAuthenticationService)
    {
        _adminService = adminService;
        _scheduleService = scheduleService;
        _busService = busService;
        _emailHelper = emailHelper;
        _customerAuthenticationService = customerAuthenticationService;
    }

    [HttpGet("get_all_customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var response = await _adminService.ListCustomers();
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("get_customer_by_id")]
    public async Task<IActionResult> GetCustomerById(CustomerInfoRequestDTO request)
    {
        var response = await _adminService.GetCustomerInfo(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("get_bus_list")]
    public async Task<IActionResult> GetBusList()
    {
        var response = await _busService.GetBusList();
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPut("assign_bus_to_driver")]
    public async Task<IActionResult> AssignBusToDriver(AssignBusToDriverRequestDTO request)
    {
        var response = await _adminService.AssignBusToDriver(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPut("assign_bus_to_schedule")]
    public async Task<IActionResult> AssignBusToSchedule (AssignBusToScheduleRequestDTO request)
    {
        var response = await _scheduleService.AssignBusToSchedule(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("send_otp")]
    public async Task<IActionResult> SendOtp (SendOtpRequestDTO request)
    {
        var response = await _emailHelper.SendOtp(request);
        return HttpResponseHelper.GetHttpResponse(response);

    }

    [HttpPut("verify_customer_email")]
    public async Task<IActionResult> VerifyCustomerOtp (EmailVerificationRequestDTO request)
    {
        var response = await _customerAuthenticationService.CustomerEmailVerification(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("get_route_stops")]
    public async Task<IActionResult> GetBusStopList (GetRouteBusStopsRequestDTO request)
    {
        var response = await _adminService.GetRouteBusStopsInfo(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }


}