using BusBooking.Core.Constants;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;

namespace BusBooking.Services.BL.Implementations;

public class AdminService : IAdminService
{
    private readonly IQueryRepository<Customer> _customerQueryRepository;

    public AdminService (IQueryRepository<Customer> customerQueryRepository)
    {
        _customerQueryRepository = customerQueryRepository;
    }

    public async Task<ApiResponse<List<CustomerListResponseDTO>>> ListCustomers()
    {
        try
        {
            var customers = await _customerQueryRepository.GetAllAsync();
            var customerList = customers.Select(s => new CustomerListResponseDTO
            {
                Email = s.Email,
                FirstName = s.FirstName,
                LastName = s.LastName,                
                Age = s.Age,
                PhoneNumber = s.PhoneNumber,
                LastLogin = s.LastLogin,
                Status = s.Status
            }).ToList();

            return ApiResponse<List<CustomerListResponseDTO>>.Success("Customers List Retrieved", customerList);
        }
        catch (Exception e)
        {
            return ApiResponse<List<CustomerListResponseDTO>>.Failure(e.Message, StatusCodes.ServerError);
        }
    }

    public async Task<ApiResponse<CustomerInfoResponseDTO>> GetCustomerInfo(CustomerInfoRequestDTO request)
    {
        try
        {
            var customer = await _customerQueryRepository.FindByIdAsync(request.CustomerId);
            if (customer == null)
                return ApiResponse<CustomerInfoResponseDTO>.Failure("Customer not found", StatusCodes.BadRequest);

            return ApiResponse<CustomerInfoResponseDTO>.Success(
                "Customer Retrieved",
                new CustomerInfoResponseDTO
                {
                    Id = customer.Id,
                    Age = customer.Age,
                    Email = customer.Email,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    Status = customer.Status
                });
        }
        catch (Exception e)
        {
            return ApiResponse<CustomerInfoResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }
}