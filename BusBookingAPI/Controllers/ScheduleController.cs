using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Controllers;

[ApiController]
[Route("schedule")]
public class ScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;
    
    public ScheduleController (IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost("create_schedule_job")]
    public async Task<IActionResult> CreateSchedule()
    {
        var response = await _scheduleService.AddScheduleJob();

        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("create_schedule_rule")]
    public async Task<IActionResult> CreateScheduleRule(AddScheduleRulesRequestDTO request)
    {
        var response = await _scheduleService.AddScheduleRuleTask(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("assign_schedule_buses")]
    public async Task<IActionResult> AssignScheduleBuses()
    {
        var response = await _scheduleService.AddBusToScheduleForTodayJob();
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpGet("get_today_schedule")]
    public async Task<IActionResult> GetTodaySchedule()
    {
        var response = await _scheduleService.GetScheduleForToday();
        return HttpResponseHelper.GetHttpResponse(response);
    }

    [HttpPost("cancel_schedule")]
    public async Task<IActionResult> CancelSchedulTask(CancelScheduleRequestDTO request)
    {
        var response = await _scheduleService.CancelSchedule(request);
        return HttpResponseHelper.GetHttpResponse(response);
    }
}