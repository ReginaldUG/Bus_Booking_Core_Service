using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
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

    [HttpPost("add_bus_stop")]
    public async Task<IActionResult> AddBusStop([FromBody] AddBusStopRequestDTO request)
    {
        var response = await _busService.AddBusStop(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }
}