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

    public RouteService(
        ICommandRepository<Route> routeCommandRepository, 
        IQueryRepository<Route> routeQueryRepository)
    {
        _routeCommandRepository = routeCommandRepository;
        _routeQueryRepository = routeQueryRepository;
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
}