using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface ITransferService
{
    Task<ApiResponse> CustomerWalletTopUp(CustomerWalletTopUpRequestDTO request);
    Task<ApiResponse<CheckWalletBalanceResponseDTO>> CheckWalletBalance();
}