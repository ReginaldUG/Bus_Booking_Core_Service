using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("route")]
public class RouteController : Controller
{
    private readonly IRouteService _routeService;

    public RouteController (IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpPost("create_route")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequestDTO request)
    {
        var response = await _routeService.CreateRouteTask(request);

        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("assign_bus")]
    public async Task<IActionResult> AssignBus([FromBody] AssignBusRequestDTO request)
    {
        var response = await _routeService.AssignBusTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("bulk_assign_bus")]
    public async Task<IActionResult> BulkAssignBus ([FromBody] BulkAssignBusesRequestDTO request)
    {
        var response = await _routeService.BulkAssignBusesToRoute(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

}