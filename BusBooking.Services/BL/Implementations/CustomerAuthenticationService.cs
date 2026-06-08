
using BusBooking.Core.Constants;
using BusBooking.Data;
using BusBooking.Data.Queries;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBookingAPI.Helpers;

namespace BusBooking.Services.BL.Implementations;

public class CustomerAuthenticationService
{
    private readonly AppDbContext _db;
    private readonly QueryRepository<Customer> _customerQueryRepository;
    private readonly AuthenticationHelper _authenticationHelper;

    public CustomerAuthenticationService(AppDbContext db, QueryRepository<Customer> customerQueryRespository, AuthenticationHelper authenticationHelper)
    {
        _db = db;
        _customerQueryRepository = customerQueryRespository;

        _authenticationHelper = authenticationHelper;
    }
    
    public async Task<ApiResponse<CustomerRegisterResponseDTO>> CustomerRegisterTask (CustomerRegisterRequestDTO registerRequest)
    {
        try
        {
            //Check that Email does not currently exist
            var customerExists = await _customerQueryRepository.FindByCriteriaAsync("Email", registerRequest.Email);
            if (customerExists != null)
            {
                return ApiResponse<CustomerRegisterResponseDTO>
                    .Failure(
                        ErrorMessages.DUPLICATE_CUSTOMER_FOUND,
                        StatusCodes.Conflict
                    );
            }

            //Validate that password meets requirements
            var passwordRulesCheck = _authenticationHelper.ValidatePasswordRules(registerRequest.Password);
            if (!passwordRulesCheck.Status)
            {
                return ApiResponse<CustomerRegisterResponseDTO>.Failure(passwordRulesCheck.Message, StatusCodes.BadRequest);
            }

            //Check that Age meets Age requirements
            if(registerRequest.Age < 18)
            {
                return ApiResponse<CustomerRegisterResponseDTO>.Failure("Must be 18 and above", StatusCodes.BadRequest);
            }

            //HashPassword
            var hashedPassword = _authenticationHelper.HashPassword(registerRequest.Password).Data;

            //PASS VALUES            
            var customer = new Customer
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Age = registerRequest.Age,
                Email = registerRequest.Email,
                HashedPassword = hashedPassword,
                Status = AccountStatus.Active,

                Wallet = new CustomerWallet
                {
                    Balance = 0
                }
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return ApiResponse<CustomerRegisterResponseDTO>.Success(
                "Customer Registration Complete",
                new CustomerRegisterResponseDTO
                {
                    Id = customer.Id,
                    Age = customer.Age,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Status = customer.Status
                }
            );

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Handle exceptions and return appropriate error response
            //Dev env: return full exception message to endpoint
            return ApiResponse<CustomerRegisterResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }

    public async Task<ApiResponse<CustomerLoginResponseDTO>> CustomerLoginTask (CustomerLoginRequestDTO loginRequest)
    {
        try
        {
            string email = loginRequest.Email;
            string password = loginRequest.Password;
            
            //Check that Passed in Email exists in DB
            var customer = await _customerQueryRepository.FindByCriteriaAsync("Email", email);
            if (customer==null)
            {
                return ApiResponse<CustomerLoginResponseDTO>
                    .Failure(
                        ErrorMessages.INVALID_CREDENTIALS,
                        StatusCodes.Unauthorized
                    );
            }
            
            //Check that Password is accurate to the existing email
            var check = _authenticationHelper.VerifyPassword(password, customer.HashedPassword);
            if (!check.Status)
            {
                return ApiResponse<CustomerLoginResponseDTO>
                    .Failure(
                        ErrorMessages.INVALID_CREDENTIALS,
                        StatusCodes.Unauthorized
                    );
            }

            //map customer to DTO for controller consumption
            CustomerLoginResponseDTO responseData = new CustomerLoginResponseDTO
            {
                Id = customer.Id,
                Age = customer.Age,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Status = customer.Status
            };
            
            return ApiResponse<CustomerLoginResponseDTO>
                .Success(
                    "Login Successful",
                    responseData
                );

        }
        catch (Exception e)
        {
            // Handle exceptions and return appropriate error response
            //Dev env: return full exception message to endpoint
            return ApiResponse<CustomerLoginResponseDTO>.Failure(e.Message, StatusCodes.ServerError );
        }
    }
}