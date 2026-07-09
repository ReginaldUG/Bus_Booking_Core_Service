using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.BL.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<List<CustomerListResponseDTO>>> ListCustomers();
    Task<ApiResponse<CustomerInfoResponseDTO>> GetCustomerInfo(CustomerInfoRequestDTO request);
}