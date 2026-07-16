using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface ITokenService
{
    Task<ApiResponse<CreateAccessTokenResponseDTO>> CreateAssignAccessToken(int customerId);
    Task<ApiResponse<VerifyAccessTokenResponseDTO>> VerifyAccessToken(VerifyAccessTokenRequestDTO request);
}