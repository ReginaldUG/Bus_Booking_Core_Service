using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;

namespace BusBooking.Services.BL.Implementations;

public class AccountEvaluationService : IAccountEvaluationService
{
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly IQueryRepository<Customer> _customerQueryRepository;
    private readonly IQueryRepository<Route> _routeQueryRepository;
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly ICommandRepository<Route> _routeCommandRepository;
    

    public AccountEvaluationService(IQueryRepository<Driver> driverQueryRepository, 
        IQueryRepository<Customer> customerQueryRepository, 
        IQueryRepository<Route> routeQueryRepository,
        IQueryRepository<Bus> busQueryRepository,
        ICommandRepository<Bus> busCommandRepository,
        ICommandRepository<Driver> driverCommandRepository,
        ICommandRepository<Route> routeCommandRepository)
    {
        _driverCommandRepository = driverCommandRepository;
        _routeCommandRepository = routeCommandRepository;
        _busCommandRepository = busCommandRepository;

        _customerQueryRepository = customerQueryRepository;
        _driverQueryRepository = driverQueryRepository;
        _routeQueryRepository = routeQueryRepository;
        _busQueryRepository = busQueryRepository;
    }
/*
    public async Task<ApiResponse> DriverActivationServiceTask()
    {
        using var transaction = _driverCommandRepository.BeginTransaction();
        try
        {
            //check for available route
            var availableRoute = await _routeQueryRepository.FindByCriteriaAsync("BusAssigned", "false");
            bool hasValidRoute = availableRoute != null && availableRoute.Id > 0;

            if (!hasValidRoute)
            {
                _driverCommandRepository.CommitTransaction(transaction);
                return ApiResponse.Success("No available routes to distribute");
            }
            
            //find oldest driver account with pending status
            var oldestPendingDriver = await _driverQueryRepository.FindByCriteriaAsync("Status", AccountStatus.Pending);
            if (oldestPendingDriver == null)
            {
                _driverCommandRepository.CommitTransaction(transaction);
                return ApiResponse.Success("No pending drivers were found");
            }

            //update driver bus with available route ID
            var driverBus = await _busQueryRepository.FindByCriteriaAsync("Id", oldestPendingDriver.BusId.ToString());
            if (driverBus == null)
            {
                _driverCommandRepository.RollbackTransaction(transaction);
                return ApiResponse.Failure("Driver Bus record not found");
            }

            driverBus.RouteId = availableRoute.Id;
            await _busCommandRepository.UpdateWithOpenDbTransactionAsync(driverBus, transaction);                
                
            //update available route to busAssigned true
            availableRoute.BusAssigned = true;
            await _routeCommandRepository.UpdateWithOpenDbTransactionAsync(availableRoute, transaction);
            
            //update driver status to active
            oldestPendingDriver.Status = AccountStatus.Active;
            await _driverCommandRepository.UpdateWithOpenDbTransactionAsync(oldestPendingDriver, transaction);
            
            _driverCommandRepository.CommitTransaction(transaction);

            return ApiResponse.Success("Status Change Successfull");
        }
        catch (Exception e)
        {
            _driverCommandRepository.RollbackTransaction(transaction);
            return ApiResponse.Failure($"Unable to update driver account : {e}");
        }
    }

*/
}