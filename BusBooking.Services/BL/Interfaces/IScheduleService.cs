using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IScheduleService
{
    Task<ApiResponse> AddScheduleJob();
    Task<ApiResponse<AddScheduleRulesResponseDTO>> AddScheduleRuleTask(AddScheduleRulesRequestDTO request);
    Task<ApiResponse> AssignBusToSchedule(AssignBusToScheduleRequestDTO request);
    Task<ApiResponse<List<GetSchedulesForDayResponseDTO>>> GetScheduleForToday();
    Task<ApiResponse> AddBusToScheduleForTodayJob();
    Task<ApiResponse> CancelSchedule(CancelScheduleRequestDTO request);
}