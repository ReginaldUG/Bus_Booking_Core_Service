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

    public AdminController (IAdminService adminService)
    {
        _adminService = adminService;
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

}