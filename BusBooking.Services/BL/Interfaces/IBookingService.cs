using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookScheduleResponseDTO>> BookSchedule(BookScheduleRequestDTO request);

    Task<ApiResponse<List<CustomerBookingBusManifestResponseDTO>>> GetCustomerBookingBusManifest(
        CustomerBookingBusManifestRequestDTO request);
    
    Task<ApiResponse<List<CustomerBookingByIdResponseDTO>>> GetCustomerBookingById(
        CustomerBookingByIdRequestDTO request);
}