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

/*BUS CREATION LOGIC
 When the create bus endpoint is triggered, the system checks if there is a RouteName passed in the body
 This RouteName would indicate that the call wants that new Bus assigned to the route
 IF NO RoutName is passed, it is assumed the Bus will stay unassigned to a Route for now

 Next step is to check if there is any driver pending a bus assignment- if so, this bus is assigned to them
 and update necessary column fields in route, bus and driver table respectively

 */

public class BusService : IBusService
{
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly IQueryRepository<Route> _routeQueryRepository;
    private readonly ICommandRepository<Route> _routeCommandRepository;
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly GeneralHelpers _generalHelpers;

    public BusService(IQueryRepository<Bus> busQueryRepository, 
        IQueryRepository<Driver> driverQueryRepository, 
        IQueryRepository<Route> routeQueryRepository,
        ICommandRepository<Bus> busCommandRepository, 
        ICommandRepository<Driver> driverCommandRepository,
        ICommandRepository<Route> routeCommandRepository,
        GeneralHelpers generalHelpers)
    {
        _busQueryRepository = busQueryRepository;
        _driverQueryRepository = driverQueryRepository;
        _routeQueryRepository = routeQueryRepository;
        _busCommandRepository = busCommandRepository;
        _driverCommandRepository = driverCommandRepository;
        _routeCommandRepository = routeCommandRepository;

        _generalHelpers = generalHelpers;
    }

    public async Task<ApiResponse<CreateBusResponseDTO>> CreateBusTask (CreateBusRequestDTO request)
    {
        try
        {
            //Ensure seat size is in the consts
            var validBusSize = ValidateSeatSize(request.BusSize.ToLower());
            if (!validBusSize.Status)
                return ApiResponse<CreateBusResponseDTO>.Failure(
                    validBusSize.Message, 
                    StatusCodes.BadRequest);
            
            //Ensure plate number is unique for bus
            var plateNumberCheck = await PlateNumberEvaluator(request.PlateNumber);
            if (!plateNumberCheck.Status)
                return ApiResponse<CreateBusResponseDTO>.Failure(
                    plateNumberCheck.Message, StatusCodes.BadRequest);

            //Ensure route Name exists
            //If exists assign to 
            Route? route = null;
            if (!string.IsNullOrWhiteSpace(request.RouteName))
            {
                var routeExists = await _routeQueryRepository.FindByCriteriaAsync("RouteName", request.RouteName);
                if (routeExists == null)
                    return ApiResponse<CreateBusResponseDTO>.Failure(
                        "Invalid Route Name", 
                        StatusCodes.BadRequest);

                route = routeExists;
            }
            
            using var transaction = _busCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //create bus entry
                var bus = new Bus
                {
                    SeatCapacity = validBusSize.Data,
                    PlateNumber = request.PlateNumber,
                    Status = BusStatus.PendingRoute,
                    RouteId = route != null ? route.Id : null
                };
                var newBus = await _busCommandRepository.AddWithOpenDBTransaction(bus, transaction);
                bool updateBus = false; //track if new bus needs to be updated in transaction

                //update route - Number of buses
                if (route!=null)
                {
                    route.NumberOfBuses++;
                    await _routeCommandRepository.UpdateWithOpenDbTransactionAsync(route, transaction);

                    //update newBus Status
                    newBus.Status = BusStatus.PendingDriver;
                    updateBus = true;
                }
                
                //attempt driver assignment for driver with status "PendingBus"
                //update bus driverAssigned column if valid
                var availableDriver = await _driverQueryRepository.FindByCriteriaAsync("Status", DriverAccountStatus.PendingBus);
                if(availableDriver != null)
                {
                    availableDriver.BusId = newBus.Id;
                    newBus.DriverAssigned = true;
                    updateBus = true;

                    if(newBus.RouteId != null)
                    {
                        availableDriver.Status = AccountStatus.Active;
                        newBus.Status = BusStatus.Active;
                    }
                    else
                    {
                        availableDriver.Status = AccountStatus.PendingRoute;
                    }                   

                    await _driverCommandRepository.UpdateWithOpenDbTransactionAsync(availableDriver, transaction);
                }

                if (updateBus)
                {
                    await _busCommandRepository.UpdateWithOpenDbTransactionAsync(newBus, transaction);
                }
                _busCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse<CreateBusResponseDTO>.Success(
                    "Bus Creation Completed", 
                    new CreateBusResponseDTO
                    {
                        BusCapacity = newBus.SeatCapacity,
                        Status = newBus.Status,
                        PlateNumber = newBus.PlateNumber
                    }
                );
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
            Console.WriteLine(e);
            return ApiResponse<CreateBusResponseDTO>.Failure(e.Message, StatusCodes.ServerError);            
        }
    }

    public async Task<ApiResponse<List<GetBusesWithoutRouteResponseDTO>>> GetBusesWithoutRoute()
    {
        //Get all buses that have no route assigned
        try
        {
            var buses = await _busQueryRepository.GetAllByCriteriaAsync("RouteId", "null");
            var responseDataList = buses.Select(bus => new GetBusesWithoutRouteResponseDTO
            {
                PlateNumber = bus.PlateNumber,
                BusCapacity = bus.SeatCapacity,
                DriverAssigned = bus.DriverAssigned
            }).ToList();

            return ApiResponse<List<GetBusesWithoutRouteResponseDTO>>
                .Success(
                    "Buses without routes retrieved successfully",
                    responseDataList);
        }
        catch (Exception e)
        {
            return ApiResponse<List<GetBusesWithoutRouteResponseDTO>>
                .Failure(e.Message, StatusCodes.ServerError);
        }        
    }

    private ApiResponse<BusCapacity> ValidateSeatSize (string size)
    {
        BusCapacity capacity;
        switch (size)
        {
            case "small":
                capacity = BusCapacity.Small;
                break;
            case "medium":
                capacity = BusCapacity.Medium;
                break;
            case "large":
                capacity = BusCapacity.Large;
                break;
            default:
                return ApiResponse<BusCapacity>.Failure(
                    ErrorMessages.INVALID_BUS_TYPE,
                    StatusCodes.BadRequest
                );
        }

        return ApiResponse<BusCapacity>.Success("Validation sucess", capacity);
    }
    private async Task<ApiResponse> PlateNumberEvaluator (string plateNumber)
    {
        var formatCheck = _generalHelpers.ValidatePlateNumberFormat(plateNumber);
        if (!formatCheck.Status)
            return ApiResponse.Failure(formatCheck.Message);

        //check that number is unique in the db
        var numberExists = await _busQueryRepository.FindByCriteriaAsync("PlateNumber", plateNumber);
        if (numberExists != null)
            return ApiResponse.Failure(ErrorMessages.DUPLICATE_PLATE_NUMBER_FOUND);

        return ApiResponse.Success("Valid plate number");
    }

}