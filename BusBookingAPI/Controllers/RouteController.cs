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

    [HttpPost("add_route_stop")]
    public async Task<IActionResult> AddRouteStop([FromBody] AddRouteBusStopRequestDTO request)
    {
        var response = await _routeService.AddRouteBusStopTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }
    

}