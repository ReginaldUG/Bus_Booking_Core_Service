using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IDriverAuthenticationService
{
    Task<ApiResponse<DriverLoginResponseDTO>> DriverLoginTask(DriverLoginRequestDTO loginRequest);
    Task<ApiResponse<DriverRegisterResponseDTO>> DriverRegisterTask (DriverRegisterRequestDTO registerRequest);
}