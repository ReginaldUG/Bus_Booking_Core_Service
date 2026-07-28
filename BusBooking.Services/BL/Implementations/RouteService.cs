using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;

namespace BusBooking.Services.BL.Implementations;

public class RouteService : IRouteService
{
    private readonly ICommandRepository<Route> _routeCommandRepository;
    private readonly IQueryRepository<Route> _routeQueryRepository;
    private readonly IQueryRepository<BusStops> _busStopQueryRepository;
    private readonly IQueryRepository<RouteStops> _routeStopQueryRepository;
    private readonly ICommandRepository<RouteStops> _routeStopCommandRepository;

    public RouteService(
        ICommandRepository<Route> routeCommandRepository, IQueryRepository<BusStops> busStopQueryRepository,
        IQueryRepository<Route> routeQueryRepository, IQueryRepository<RouteStops> routeStopQueryRepository,
        ICommandRepository<RouteStops> routeStopCommandRepository)
    {
        _routeCommandRepository = routeCommandRepository;
        _routeQueryRepository = routeQueryRepository;
        _busStopQueryRepository = busStopQueryRepository;
        _routeStopQueryRepository = routeStopQueryRepository;
        _routeStopCommandRepository = routeStopCommandRepository;
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

            var route = new Route
            {
                RouteName = request.RouteName,
                Price = request.Price
            };
            await _routeCommandRepository.AddAsync(route);

            return ApiResponse<CreateRouteResponseDTO>.Success(
                "Route Added Successfully", 
                new CreateRouteResponseDTO
                {
                    RouteName = route.RouteName
                }
            );
        }
        catch (Exception e)
        {
            return ApiResponse<CreateRouteResponseDTO>.Failure(
                $"{e}", StatusCodes.ServerError);
        }
    }

    //Add new Route Stop (route bus stop relationship)
    public async Task<ApiResponse<AddRouteBusStopResponseDTO>> AddRouteBusStopTask (AddRouteBusStopRequestDTO request)
    {
        try
        {
            //check that route exists
            var busStop = await _busStopQueryRepository.FindByIdAsync(request.BusStopId);
            if (busStop == null)
                return ApiResponse<AddRouteBusStopResponseDTO>.Failure(ErrorMessages.BUS_STOP_NOT_FOUND,
                    StatusCodes.BadRequest);
            var route = await _routeQueryRepository.FindByIdAsync(request.RouteId);
            if (route == null)
                return ApiResponse<AddRouteBusStopResponseDTO>.Failure(ErrorMessages.ROUTE_NOT_FOUND,
                    StatusCodes.BadRequest);
            
            //check that bus stop and route dont already exist
            var searchParams = new Dictionary<string, object>
            {
                { nameof(RouteStops.BusStopId), request.BusStopId },
                { nameof(RouteStops.RouteId), request.RouteId }
            };
            var exist = await _routeStopQueryRepository.FindByMultipleFieldsAsync(searchParams, 1);
            if (exist.Any())
                return ApiResponse<AddRouteBusStopResponseDTO>.Failure(ErrorMessages.DUPLICATE_ENTRY, StatusCodes.Conflict);
            
            //save
            var newRouteStop = new RouteStops
            {
                BusStopId = request.BusStopId,
                RouteId = request.RouteId
            };
            await _routeStopCommandRepository.AddAsync(newRouteStop);

            return ApiResponse<AddRouteBusStopResponseDTO>.Success("New Stop Added to Route", new AddRouteBusStopResponseDTO
            {
                BusStopName = busStop.Name,
                RouteName = route.RouteName
            });
        }
        catch (Exception e)
        {
            return ApiResponse<AddRouteBusStopResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
        
    }
}