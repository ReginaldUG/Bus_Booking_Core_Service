using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;

namespace BusBooking.Services.BL.Implementations;

public class DriverAuthenticationService : IDriverAuthenticationService
{
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly IQueryRepository<Route> _routeQueryRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Route> _routeCommandRepository;
    private readonly AuthenticationHelper _authenticationHelper;

    public DriverAuthenticationService(IQueryRepository<Driver> driverQueryRepository, IQueryRepository<Route> routeQueryRepository, ICommandRepository<Driver> driverCommandRepository, ICommandRepository<Bus> busCommandRepository, ICommandRepository<Route> routeCommandRespository, AuthenticationHelper authenticationHelper)
    {
        _driverQueryRepository = driverQueryRepository;
        _routeQueryRepository = routeQueryRepository;
        _driverCommandRepository = driverCommandRepository;
        _busCommandRepository = busCommandRepository;
        _routeCommandRepository = routeCommandRespository;

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
            //check that driver email already exists
            var driverExists = await _driverQueryRepository.FindByCriteriaAsync("Email", registerRequest.Email);
            if (driverExists == null)
            {
                return ApiResponse<DriverRegisterResponseDTO>
                    .Failure(
                        ErrorMessages.DUPLICATE_DRIVER_FOUND,
                        StatusCodes.Conflict
                    );
            }
            //ensure password meets criteria
            var passwordCheck = _authenticationHelper.ValidatePasswordRules(registerRequest.Password);
            if (!passwordCheck.Status)
            {
                return ApiResponse<DriverRegisterResponseDTO>.Failure(passwordCheck.Message, StatusCodes.BadRequest);
            }

            //check age
            if(registerRequest.Age < 25)
            {
                return ApiResponse<DriverRegisterResponseDTO>.Failure("Must be 25 and above", StatusCodes.BadRequest);
            }

            //hash password
            var hashedPassword = _authenticationHelper.HashPassword(registerRequest.Password).Data;

            using var transaction = _driverCommandRepository.BeginTransaction();
            try
            {
                //create bus
                BusCapacity[] capacities = Enum.GetValues<BusCapacity>();
                BusCapacity randomSize = capacities[Random.Shared.Next(capacities.Length)];
                var availableRoute = await _routeQueryRepository.FindByCriteriaAsync("BusAssigned", "false");

                //Create new bus for driver
                var bus = new Bus
                {
                    SeatCapacity = randomSize,
                    RouteId = availableRoute?.Id
                };
                int busId = await _busCommandRepository.AddWithOpenDBTransaction(bus, transaction);

                //Update Route BusAssigned Flag if route ewas given to bus
                if (availableRoute != null)
                {
                    availableRoute.BusAssigned = true;
                    await _routeCommandRepository.UpdateWithOpenDbTransactionAsync(availableRoute, transaction);
                }

                //create driver after bus creation
                var driver = new Driver
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Age = registerRequest.Age,
                    Email = registerRequest.Email,
                    HashedPassword = hashedPassword,
                    BusId = busId,
                    Status = availableRoute==null ? AccountStatus.Pending : AccountStatus.Active
                };
                await _driverCommandRepository.AddWithOpenDBTransaction(driver, transaction);

                //commit the transaction
                _driverCommandRepository.CommitTransaction(transaction);

                return ApiResponse<DriverRegisterResponseDTO>.Success(
                    "Driver Registration Completed",
                    new DriverRegisterResponseDTO
                    {
                        FirstName = driver.FirstName,
                        LastName = driver.LastName,
                        BusAssigned = driver.BusId != 0,
                        Status = driver.Status
                    }
                );
            }
            catch (Exception e)
            {
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
}
