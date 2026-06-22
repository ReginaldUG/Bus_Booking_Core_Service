using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IBusService
{
    Task<ApiResponse<CreateBusResponseDTO>> CreateBusTask(CreateBusRequestDTO request);
    Task<ApiResponse<List<GetBusesWithoutRouteResponseDTO>>> GetBusesWithoutRoute();
}