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

public class AdminService : IAdminService
{
    private readonly IQueryRepository<Customer> _customerQueryRepository;
    private readonly IQueryRepository<CustomerWallet> _walletQueryRepository;
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;

    public readonly GeneralHelpers _generalHelpers;

    public AdminService(IQueryRepository<Customer> customerQueryRepository, IQueryRepository<CustomerWallet> walletQueryRepository,
        IQueryRepository<Bus> busQueryRepository, IQueryRepository<Driver> driverQueryRepository, 
        ICommandRepository<Bus> busCommandRepository, ICommandRepository<Driver> driverCommandRepository,
        GeneralHelpers generalHelpers)
    {
        _customerQueryRepository = customerQueryRepository;
        _walletQueryRepository = walletQueryRepository;
        _driverQueryRepository = driverQueryRepository;
        _busQueryRepository = busQueryRepository;
        _busCommandRepository = busCommandRepository;
        _driverCommandRepository = driverCommandRepository;
        _generalHelpers = generalHelpers;
    }

    //ASSIGN BUS TO DRIVER
    public async Task<ApiResponse> AssignBusToDriver(AssignBusToDriverRequestDTO request)
    {
        try
        {
            //Validation 1: check plateNumber exists and bus is unassigned
            var searchParams = new Dictionary<string, object>
            {
                { nameof(Bus.PlateNumber), request.PlateNumber },
                { nameof(Bus.DriverAssigned), false }
            };
            var bus = (await _busQueryRepository.FindByMultipleFieldsAsync(searchParams, null)).FirstOrDefault();
            if (bus == null)
                return ApiResponse.Failure("Unassigned Bus not found");
            
            //Validation 2: Ensure driver exists and has no bus assigned
            var driverParams = new Dictionary<string, object>
            {
                { nameof(Driver.Id), request.DriverId.ToString() },
                { nameof(Driver.BusId), "null" }
            };
            var driver = (await _driverQueryRepository.FindByMultipleFieldsAsync(driverParams, null)).FirstOrDefault();
            if (driver == null)
                return ApiResponse.Failure("Unassigned Driver not found");
            
            //update
            using var transaction = _busCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //update bus table
                bus.DriverAssigned = true;
                bus.Status = BusStatus.Active;

                //Update drvier table
                driver.BusId = bus.Id;
                driver.Status = DriverAccountStatus.Active;

                //Save updates
                await _busCommandRepository.UpdateWithOpenDbTransactionAsync(bus, transaction);
                await _driverCommandRepository.UpdateWithOpenDbTransactionAsync(driver, transaction);

                _busCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success($"Driver {driver.FirstName} has been assigned to Bus {bus.PlateNumber}");
            }
            catch (Exception e)
            {
                if(!isCommitted)
                    _busCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
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
            var wallet = await _walletQueryRepository.FindByCriteriaAsync(nameof(CustomerWallet.CustomerId), customer.Id.ToString());
            if (wallet == null)
                return ApiResponse<CustomerInfoResponseDTO>.Failure("Wallet not found", StatusCodes.BadRequest);

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
                    Status = customer.Status,
                    WalletBalance = $"NGN {wallet.Balance}"
                });
        }
        catch (Exception e)
        {
            return ApiResponse<CustomerInfoResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }
}