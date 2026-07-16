using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IScheduleService _scheduleService;
    private readonly IBusService _busService;

    public AdminController (IAdminService adminService, IBusService busService, IScheduleService scheduleService)
    {
        _adminService = adminService;
        _scheduleService = scheduleService;
        _busService = busService;
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

}