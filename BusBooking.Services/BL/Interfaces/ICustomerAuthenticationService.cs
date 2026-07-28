using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface ICustomerAuthenticationService
{
    Task<ApiResponse<CustomerRegisterResponseDTO>> CustomerRegisterTask(CustomerRegisterRequestDTO registerRequest);
    Task<ApiResponse<CustomerLoginResponseDTO>> CustomerLoginTask(CustomerLoginRequestDTO loginRequest);
    Task<ApiResponse> CustomerLogOut(CustomerLogOutRequestDTO request);
    Task<ApiResponse<EditCustomerDetailsResponseDTO>> EditCustomerInformation(EditCustomerDetailsRequestDTO request);
    Task<ApiResponse> CustomerEmailVerification(EmailVerificationRequestDTO request);
}