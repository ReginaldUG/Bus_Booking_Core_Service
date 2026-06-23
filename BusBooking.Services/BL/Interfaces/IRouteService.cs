using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IRouteService
{
    Task<ApiResponse<CreateRouteResponseDTO>> CreateRouteTask(CreateRouteRequestDTO request);
    Task<ApiResponse> AssignBusTask(AssignBusRequestDTO request);
    Task<ApiResponse> AssignBusesByCount(AssignBusesByCountRequestDTO request);
    Task<ApiResponse> AssignBusesByPlates(AssignBusesByPlatesRequestDTO request);
}