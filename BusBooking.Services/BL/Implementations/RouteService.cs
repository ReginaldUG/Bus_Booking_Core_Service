using System.Data;
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
    private readonly IAccountEvaluationService _driverActivationService;

    public RouteService(ICommandRepository<Route> routeCommandRepository, IQueryRepository<Route> routeQueryRepository, IAccountEvaluationService accountEvaluationService)
    {
        _routeCommandRepository = routeCommandRepository;
        _routeQueryRepository = routeQueryRepository;
        _driverActivationService = accountEvaluationService;
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
            if (!RouteType.AllRouteTypes.Contains(request.Type))
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    "Invalid Route Type provided", StatusCodes.BadRequest);
            
            //Ensure Deprature time meets requiremnts
            if (request.DepartureTime < Rules.START_TIME || request.DepartureTime > Rules.END_TIME)
                return ApiResponse<CreateRouteResponseDTO>.Failure(
                    "DepartureTime must be between 7:00 - 21:00", StatusCodes.BadRequest);

            var route = new Route
            {
                RouteName = request.RouteName,
                Price = request.Price,
                Type = request.Type,
                DepartureTime = request.DepartureTime
            };
            await _routeCommandRepository.AddAsync(route);

            //Initiate driver account evaluation
            var result = await _driverActivationService.DriverActivationServiceTask();

            string message = 
                result.Status 
                    ? $"Route added Successfully. Driver matching detail: {result.Message}" 
                    : $"Route added Successfully but Driver Matching encountered an issue : {result.Message}";

            return ApiResponse<CreateRouteResponseDTO>.Success(
                message, 
                new CreateRouteResponseDTO
                {
                    RouteName = route.RouteName,
                    Type = route.Type,
                    DepartureTime = route.DepartureTime
                }
            );
        }
        catch (Exception e)
        {
            return ApiResponse<CreateRouteResponseDTO>.Failure(
                $"{e}", StatusCodes.ServerError);
        }
    }

    
}