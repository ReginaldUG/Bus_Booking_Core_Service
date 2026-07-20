using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("booking")]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("create_booking")]
    public async Task<IActionResult> CreateBooking([FromBody] BookScheduleRequestDTO request)
    {
        var response = await _bookingService.BookSchedule(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("get_customer_booking_by_id")]
    public async Task<IActionResult> GetCustomerBookingById([FromBody] CustomerBookingByIdRequestDTO request)
    {
        var response = await _bookingService.GetCustomerBookingById(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("booking_manifest")]
    public async Task<IActionResult> GetCustomerBookingBusManifest([FromBody] CustomerBookingBusManifestRequestDTO request)
    {
        var response = await _bookingService.GetCustomerBookingBusManifest(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPut("cancel_booking")]
    public async Task<IActionResult> CancelCustomerBooking([FromBody] CancelCustomerBookingRequestDTO request)
    {
        var response = await _bookingService.CancelCustomerBooking(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }
}