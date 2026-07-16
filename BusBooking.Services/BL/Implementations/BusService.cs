using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
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
    private readonly ICommandRepository<Bus> _busCommandRepository;
    private readonly ICommandRepository<Driver> _driverCommandRepository;
    private readonly GeneralHelpers _generalHelpers;

    public BusService(IQueryRepository<Bus> busQueryRepository,
        IQueryRepository<Driver> driverQueryRepository,
        ICommandRepository<Bus> busCommandRepository,
        ICommandRepository<Driver> driverCommandRepository,
        GeneralHelpers generalHelpers)
    {
        _busQueryRepository = busQueryRepository;
        _driverQueryRepository = driverQueryRepository;
        _busCommandRepository = busCommandRepository;
        _driverCommandRepository = driverCommandRepository;

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
            
            //CreateBus
            var newBus = new Bus
            {
                SeatCapacity = validBusSize.Data,
                Status = BusStatus.PendingDriver,
                PlateNumber = request.PlateNumber
            };
            await _busCommandRepository.AddAsync(newBus);

            return ApiResponse<CreateBusResponseDTO>.Success("Bus Created", new CreateBusResponseDTO
            {
                PlateNumber = request.PlateNumber,
                BusCapacity = validBusSize.Data,                
                Status = BusStatus.PendingDriver
            });
        }
        catch (Exception e)
        {
            return ApiResponse<CreateBusResponseDTO>.Failure(e.Message, StatusCodes.ServerError);            
        }
    }

    public async Task<ApiResponse<List<GetBusListResponseDTO>>> GetBusList()
    {
        try
        {
            //get all bus list
            var buses = (await _busQueryRepository.GetAllAsync()).ToList();
            if (!buses.Any())
                return ApiResponse<List<GetBusListResponseDTO>>.Success("No Buses Found", null);

            var drivers = await _driverQueryRepository.GetAllAsync();
            var driverMapping = drivers.Where(d => d.BusId.HasValue).ToDictionary(d => d.BusId!.Value);

            var busList = buses.Select(b =>
            {
                driverMapping.TryGetValue(b.Id, out var driver);
                return new GetBusListResponseDTO
                {
                    BusId = b.Id,
                    PlateNumber = b.PlateNumber,
                    DriverEmail = driver?.Email,
                    SeatCapacity = b.SeatCapacity,
                    Status = b.Status
                };
            }).ToList();

            return ApiResponse<List<GetBusListResponseDTO>>.Success("Bus List Retrieved", busList);
        }
        catch (Exception e)
        {
            return ApiResponse<List<GetBusListResponseDTO>>.Failure(e.Message, StatusCodes.ServerError);
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