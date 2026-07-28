using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBooking.Services.Helpers;

namespace BusBooking.Services.BL.Implementations;

public class DriverAuthenticationService : IDriverAuthenticationService
{
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly AuthenticationHelper _authenticationHelper;

    public DriverAuthenticationService(
        IQueryRepository<Driver> driverQueryRepository,
        ICommandRepository<Driver> driverCommandRepository,
        AuthenticationHelper authenticationHelper)
    {
        _driverQueryRepository = driverQueryRepository;
        _driverCommandRepository = driverCommandRepository;

        _authenticationHelper = authenticationHelper;
    }

    public async Task<ApiResponse<DriverLoginResponseDTO>> DriverLoginTask (DriverLoginRequestDTO loginRequest)
    {
        try
        {
            string email = loginRequest.Email;
            string password = loginRequest.Password;

            var driver = await _driverQueryRepository.FindByCriteriaAsync("Email", email);
            if (driver == null)
            {
                return ApiResponse<DriverLoginResponseDTO>
                    .Failure(
                        ErrorMessages.INVALID_CREDENTIALS,
                        StatusCodes.Unauthorized
                    );
            }
            //check password
            var passwordCheck = _authenticationHelper.VerifyPassword(password, driver.HashedPassword);
            if (!passwordCheck.Status)
            {
                return ApiResponse<DriverLoginResponseDTO>
                    .Failure(
                        ErrorMessages.INVALID_CREDENTIALS,
                        StatusCodes.Unauthorized
                    );
            }

            driver.LastLogin = DateTime.UtcNow;
            await _driverCommandRepository.UpdateAsync(driver);

            return ApiResponse<DriverLoginResponseDTO>.Success(
                "Login Successful",
                new DriverLoginResponseDTO
                {
                    Id = driver.Id,
                    Age = driver.Age,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    Status = driver.Status
                }
            );
        }
        catch (Exception e)
        {
            return ApiResponse<DriverLoginResponseDTO>.Failure(e.Message, StatusCodes.ServerError );
        }
    }
        
    public async Task<ApiResponse<DriverRegisterResponseDTO>> DriverRegisterTask (DriverRegisterRequestDTO registerRequest)
    {
        try
        {
            //check that email and number inputs are valid
            var validateEmailNumberInputs = await ValidateDriverRegInputs(registerRequest);
            if (!validateEmailNumberInputs.Status)
            {
                return ApiResponse<DriverRegisterResponseDTO>
                    .Failure(
                        validateEmailNumberInputs.Message, StatusCodes.BadRequest
                    );
            }

            var validatePasswordAndAgeInputs = await PasswordAndAgeCheck(registerRequest);
            if (!validatePasswordAndAgeInputs.Status)
            {
                return ApiResponse<DriverRegisterResponseDTO>
                    .Failure(
                        validatePasswordAndAgeInputs.Message, validatePasswordAndAgeInputs.StatusCode
                    );
            }
            //pass in hashedPassword from validate method response
            string hashedPassword = validatePasswordAndAgeInputs.Data;

            using var transaction = _driverCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //create driver
                var driver = new Driver
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Age = registerRequest.Age,
                    Email = registerRequest.Email,
                    PhoneNumber = registerRequest.PhoneNumber,
                    HashedPassword = hashedPassword,
                    Status = DriverAccountStatus.PendingBus
                };
                await _driverCommandRepository.AddWithOpenDBTransaction(driver, transaction);

                _driverCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse<DriverRegisterResponseDTO>.Success(
                    "Driver Registration Completed",
                    new DriverRegisterResponseDTO
                    {
                        FirstName = driver.FirstName,
                        LastName = driver.LastName,
                        PhoneNumber = driver.PhoneNumber,
                        Status = driver.Status
                    }
                );

            }
            catch (Exception e)
            {
                if(!isCommitted)
                    _driverCommandRepository.RollbackTransaction(transaction);
                
                throw;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiResponse<DriverRegisterResponseDTO>.Failure(e.Message, StatusCodes.ServerError);            
        }
    }
    
    private async Task<ApiResponse> ValidateDriverRegInputs (DriverRegisterRequestDTO request)
    {
        try
        {
            var driverExists = await _driverQueryRepository.FindByCriteriaAsync("Email", request.Email);
            if (driverExists != null)
            {
                return ApiResponse
                    .Failure(
                        ErrorMessages.DUPLICATE_DRIVER_FOUND
                    );
            }

            //check phone number passed is valid number
            if (!request.PhoneNumber.All(char.IsDigit))
            {
                return ApiResponse
                    .Failure(
                        "Phone Number Invalid"
                    );
            }

            var numberExist = await _driverQueryRepository.FindByCriteriaAsync("PhoneNumber", request.PhoneNumber);
            if(numberExist != null)
            {
                return ApiResponse.Failure(ErrorMessages.DUPLICATE_PHONE_NUMBER_FOUND);
            }

            return ApiResponse.Success("Input Validation Passed");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiResponse.Failure(e.Message);
        }
    }

    private async Task<ApiResponse<string>> PasswordAndAgeCheck (DriverRegisterRequestDTO request)
    {
        try
        {
            //ensure password meets criteria
            var passwordCheck = _authenticationHelper.ValidatePasswordRules(request.Password);

            string message = !passwordCheck.Status ? passwordCheck.Message :
                request.Age < Rules.MIN_DRIVER_AGE ? $"Must be {Rules.MIN_DRIVER_AGE} and above" :
                request.Age > Rules.MAX_DRIVER_AGE ? $"Cannot be above {Rules.MAX_DRIVER_AGE} yrs" : "good";

            if (message != "good")
                return ApiResponse<string>.Failure(message, StatusCodes.BadRequest);
            
            //hash password
            string hashedPassword = _authenticationHelper.HashPassword(request.Password).Data;

            return ApiResponse<string>.Success("Password Age validation success", hashedPassword);
        }
        catch (Exception e)
        {
            return ApiResponse<string>.Failure(e.Message, StatusCodes.BadRequest);
        }
    }
        
    
}
