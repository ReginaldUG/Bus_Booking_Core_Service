using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<List<GetAllActiveScheduleForTodayResponseDTO>>> GetAllActiveScheduleForToday();
    Task<ApiResponse<BookScheduleResponseDTO>> BookSchedule(BookScheduleRequestDTO request);
}