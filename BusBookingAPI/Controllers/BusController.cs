using System.Net;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Services.BL.Implementations;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("bus")]
public class BusController : Controller
{
    private readonly IBusService _busService;

    public BusController(IBusService busService)
    {
        _busService = busService;
    }

    [HttpPost("create_bus")]
    public async Task<IActionResult> CreateBus([FromBody] CreateBusRequestDTO request)
    {
        var response = await _busService.CreateBusTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("no_route")]
    public async Task<IActionResult> GetBusesWithoutRoute()
    {
        var response = await _busService.GetBusesWithoutRoute();
        return HttpResponseHelper.GetHttpResponse(response);
    }
}