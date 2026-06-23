using System.Data;
using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Npgsql;

namespace BusBooking.Services.BL.Implementations;

public class RouteService : IRouteService
{
    private readonly ICommandRepository<Route> _routeCommandRepository;
    private readonly IQueryRepository<Route> _routeQueryRepository;
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly IQueryRepository<Driver> _driverQueryRepository;
    private readonly GeneralHelpers _generalHelpers;

    public RouteService(
        ICommandRepository<Route> routeCommandRepository, ICommandRepository<Bus> busCommandRepository, 
        ICommandRepository<Driver> driverCommandRepository, IQueryRepository<Bus> busQueryRepository, 
        IQueryRepository<Route> routeQueryRepository, IQueryRepository<Driver> driverQueryRepository, GeneralHelpers generalHelpers)
    {
        _routeCommandRepository = routeCommandRepository;
        _busCommandRepository = busCommandRepository;
        _driverCommandRepository = driverCommandRepository;
        _routeQueryRepository = routeQueryRepository;
        _driverQueryRepository = driverQueryRepository;
        _busQueryRepository = busQueryRepository;
        
        _generalHelpers = generalHelpers;
    }

    public async Task<ApiResponse<CreateRouteResponseDTO>> CreateRouteTask (CreateRouteRequestDTO request)
    {
        try
        {
            //Check that route name does not already exist
            var routeNameExists = await _routeQueryRepository.FindByCriteriaAsync("RouteName", request.RouteName);
            if (routeNameExists != null)
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    ErrorMessages.DUPLICATE_ROUTE_FOUND, StatusCodes.Conflict);
            
            //Ensure route price meets the minimum
            if (request.Price < Rules.MIN_ROUTE_PRICE)
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    "Price cannot be less than minimum", StatusCodes.BadRequest);
            
            //Ensure route type is valid
            var normalizedType = request.Type.ToLower();
            if (!RouteType.AllRouteTypes.Contains(normalizedType))
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    "Invalid Route Type provided", StatusCodes.BadRequest);
            
            //Ensure Deprature time meets requiremnts
            if (request.DepartureTime < Rules.START_TIME || request.DepartureTime > Rules.END_TIME)
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    "DepartureTime must be between 7:00 - 21:00", StatusCodes.BadRequest);


            //Block entry if time and type are not correct
            if (normalizedType == RouteType.Morning)
            {
                if(request.DepartureTime >= Rules.MorningCutOff)
                {
                    return ApiResponse<CreateRouteResponseDTO>.Failure(
                        "Morning route must depart before 12pm",
                        StatusCodes.BadRequest
                        );
                }
                else if(normalizedType == RouteType.Evening)
                {
                    if (request.DepartureTime < Rules.EveningStartTime)
                        return ApiResponse<CreateRouteResponseDTO>.Failure(
                            "Evening route cannot depart before 4pm", 
                            StatusCodes.BadRequest
                        );
                }
            }

            var route = new Route
            {
                RouteName = request.RouteName,
                Price = request.Price,
                Type = request.Type,
                DepartureTime = request.DepartureTime
            };
            await _routeCommandRepository.AddAsync(route);

            return ApiResponse<CreateRouteResponseDTO>.Success(
                "Route Added Successfully", 
                new CreateRouteResponseDTO
                {
                    RouteName = route.RouteName,
                    Type = route.Type,
                    DepartureTime = route.DepartureTime,
                    NumberOfBuses = route.NumberOfBuses
                }
            );
        }
        catch (Exception e)
        {
            return ApiResponse<CreateRouteResponseDTO>.Failure(
                $"{e}", StatusCodes.ServerError);
        }
    }

    //Manually Assign Bus to route (1 bus at a time assigning - Bus Plate number and Route Name)
    public async Task<ApiResponse> AssignBusTask (AssignBusRequestDTO request)
    {
        try
        {
            var verifyInputs = await SingleBusAssignInputVerification(request);
            if (!verifyInputs.Status)
                return ApiResponse.Failure(verifyInputs.Message);

            var busToAssign = verifyInputs.Data.BusToAssign;
            var routeToAssign = verifyInputs.Data.RouteToAssign;
            var busDriver = verifyInputs.Data.BusDriver;

            //HANDLE SCENARIO TRANSACTION
            using var transaction = _routeCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                await BusRouteAssigningAsync(transaction, busToAssign, routeToAssign, busDriver);

                _routeCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success("Bus Assigned to Route successfully");
            }
            catch (Exception e)
            {
                if(!isCommitted)
                    _routeCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }        
    }

    //Manual Assign Bus to route (multi bus at a time - Number of buses to assign and Route Name)
    public async Task<ApiResponse> AssignBusesByCount (AssignBusesByCountRequestDTO request)
    {
        try
        {
            //Validation: Ensure number passed in is valid
            if (request.NumberOfBuses <= 0)
                return ApiResponse.Failure(ErrorMessages.INVALID_CREDENTIALS);
            
            //ensure routeName passed exists in db
            var routeToAssign = await _routeQueryRepository.FindByCriteriaAsync("RouteName", request.RouteName);
            if (routeToAssign == null)
                return ApiResponse.Failure(ErrorMessages.ROUTE_NOT_FOUND);

            //Fetch available buses within the requested limit
            var busesToAssign = (await _busQueryRepository.GetLimitedByCriteriaAsync("RouteId", "null", request.NumberOfBuses)).ToList();
            if (!busesToAssign.Any())
                return ApiResponse.Failure("No Unassigned Buses crurently available");

            //Validation: Ensure requested number of buses are available to be assigned
            if (request.NumberOfBuses > busesToAssign.Count)
                return ApiResponse.Failure($"Requested {request.NumberOfBuses}, But only {busesToAssign.Count} unassigned Buses available");

            //HANDLE SCENARIO
            using var transaction = _routeCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                Driver? busDriver = null;
                //Begin loop
                foreach (var bus in busesToAssign)
                {
                    //Get bus driver
                    if (bus.DriverAssigned)
                    {
                        busDriver = await _driverQueryRepository.FindByCriteriaAsync("BusId", bus.Id.ToString());
                    }   
                    //pass values into async function
                    await BusRouteAssigningAsync(transaction, bus, routeToAssign, busDriver);
                }
                //commit transactin outside loop
                _routeCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success("Buses Assigned to Route");
            }
            catch (Exception e)
            {
                if(!isCommitted)
                    _routeCommandRepository.RollbackTransaction(transaction);

                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    public async Task<ApiResponse> AssignBusesByPlates (AssignBusesByPlatesRequestDTO request)
    {
        try
        {
            //verify the route name
            var routeToAssign = await _routeQueryRepository.FindByCriteriaAsync("RouteName", request.RouteName);
            if (routeToAssign == null)
                return ApiResponse.Failure(ErrorMessages.ROUTE_NOT_FOUND);
            
            //fetch all buses
            var busesList = (await _busQueryRepository.FindAllByMultipleValuesAsync("PlateNumber", request.PlateNumbers)).ToList();

            //Validation: Ensure all requested plates exist
            if (busesList.Count != request.PlateNumbers.Count)
                return ApiResponse.Failure("One or more plate numbers provided do not exist");
            
            //Validation: Prevent changing buses with existing routes assigned
            if (busesList.Any(b => b.RouteId != null))
                return ApiResponse.Failure("One or more selected buses alreadyn assigned to route");

            //Initiate the transaction
            using var transaction = _routeCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //loop to go through the buesesList
                foreach (var bus in busesList)
                {
                    Driver? busDriver = null;
                    //extract driver if any assigned
                    if (bus.DriverAssigned)
                    {
                        busDriver = await _driverQueryRepository.FindByCriteriaAsync("BusId", bus.Id.ToString());
                    }                  

                    //execute updates
                    await BusRouteAssigningAsync(transaction, bus, routeToAssign, busDriver);
                }
                //commit transaction outside loop
                _routeCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success("All buses assigned to route");
            }
            catch (Exception e)
            {
                if(!isCommitted)
                    _routeCommandRepository.RollbackTransaction(transaction);
                
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    private async Task BusRouteAssigningAsync (NpgsqlTransaction sqlTransaction, 
        Bus busToAssign, Route routeToAssign, Driver? busDriver)
    {
        //  Update bus with new route ID
        //  Update bus status if bus has assigned driver
        busToAssign.RouteId = routeToAssign.Id;
        busToAssign.Status = busToAssign.DriverAssigned ? BusStatus.Active : BusStatus.PendingDriver;

        await _busCommandRepository.UpdateWithOpenDbTransactionAsync(busToAssign, sqlTransaction);

        //update driver status
        if(busDriver != null)
        {
            busDriver.Status = DriverAccountStatus.Active;
                await _driverCommandRepository.UpdateWithOpenDbTransactionAsync(busDriver, sqlTransaction);
        }

        //update route number of buses count
        routeToAssign.NumberOfBuses++;
        await _routeCommandRepository.UpdateWithOpenDbTransactionAsync(routeToAssign, sqlTransaction);
        
    }
    

    //INTERNAL DTOs
    internal class SingleBusAssignInputVerificationDTO
    {
        internal Bus BusToAssign { get; set; }
        internal Route RouteToAssign { get; set; }
        internal Driver? BusDriver { get; set; }
    }

    //PRIVATE HELPER FUNCTIONS
    private async Task<ApiResponse<SingleBusAssignInputVerificationDTO>> SingleBusAssignInputVerification (AssignBusRequestDTO request)
    {
        try
        {
            //Validate plate number
            var formatCheck = _generalHelpers.ValidatePlateNumberFormat(request.BusPlateNumber);
            if (!formatCheck.Status)
                return ApiResponse<SingleBusAssignInputVerificationDTO>.Failure(formatCheck.Message, StatusCodes.BadRequest);
            
            //Check that plate number exists and get bus
            
            var busToAssign = await _busQueryRepository.FindByCriteriaAsync("PlateNumber", request.BusPlateNumber);
            if (busToAssign == null)
            {
                return ApiResponse<SingleBusAssignInputVerificationDTO>.Failure(ErrorMessages.BUS_NOT_FOUND,StatusCodes.BadRequest);
            }
            //Check if bus with number does not have a route assigned yet
            if (busToAssign.RouteId != null)
            {
                return ApiResponse<SingleBusAssignInputVerificationDTO>.Failure("Bus already has assigned route",StatusCodes.BadRequest);
            }

            //if bus exists, check and get its assigned driver if one is assigned
            Driver? busDriver = null;
            if (busToAssign.DriverAssigned)
            {
                busDriver = await _driverQueryRepository.FindByCriteriaAsync("BusId", busToAssign.Id.ToString());
            }

            //check that routeName exists
            var routeToAssign = await _routeQueryRepository.FindByCriteriaAsync("RouteName", request.RouteName);
            if(routeToAssign == null)
            {
                return ApiResponse<SingleBusAssignInputVerificationDTO>.Failure(ErrorMessages.ROUTE_NOT_FOUND, StatusCodes.BadRequest);
            }

            return ApiResponse<SingleBusAssignInputVerificationDTO>
                .Success("Single Bus Assign Inputs valid, proceed", 
                    new SingleBusAssignInputVerificationDTO
                    {
                        BusToAssign = busToAssign,
                        RouteToAssign = routeToAssign,
                        BusDriver = busDriver
                    });
        }
        catch (Exception e)
        {
            return ApiResponse<SingleBusAssignInputVerificationDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }
}