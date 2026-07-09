using System.Data;
using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Extensions;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBookingAPI.Helpers;
using Npgsql.Replication;

namespace BusBooking.Services.BL.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly IQueryRepository<Schedule> _scheduleQueryRepository;
    private readonly IQueryRepository<ScheduleRules> _rulesQueryRepository;
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly IQueryRepository<Route> _routQueryRepository;
    private readonly ICommandRepository<Schedule> _scheduleCommandRepository;
    private readonly ICommandRepository<ScheduleRules> _rulesCommandRepository;
    private readonly GeneralHelpers _generalHelpers;

    public ScheduleService (IQueryRepository<Schedule> scheduleQueryRepository, 
        IQueryRepository<ScheduleRules> rulesQueryRepository, IQueryRepository<Bus> busQueryRepository, IQueryRepository<Route> routQueryRepository,
        ICommandRepository<Schedule> scheduleCommandRepository,
        ICommandRepository<ScheduleRules> rulesCommandRepository,
        GeneralHelpers generalHelpers)
    {
        _scheduleQueryRepository = scheduleQueryRepository;
        _rulesQueryRepository = rulesQueryRepository;
        _routQueryRepository = routQueryRepository;
        _busQueryRepository = busQueryRepository;
        _rulesCommandRepository = rulesCommandRepository;
        _scheduleCommandRepository = scheduleCommandRepository;
        _generalHelpers = generalHelpers;
    }

    //Service to add schedule rules
    public async Task<ApiResponse<AddScheduleRulesResponseDTO>> AddScheduleRuleTask (AddScheduleRulesRequestDTO request)
    {
        try
        {
            //check that Id inputed exists
            var route = await _routQueryRepository.FindByIdAsync(request.RouteId);
            if(route == null)
            {
                return ApiResponse<AddScheduleRulesResponseDTO>.Failure(
                    ErrorMessages.ROUTE_NOT_FOUND, StatusCodes.BadRequest);
            }
            
            //Validation: Ensure schedule time is between valid operating hours
            if (request.ScheduledDepartureTime < Rules.START_TIME || request.ScheduledDepartureTime > Rules.END_TIME)
            {
                return ApiResponse<AddScheduleRulesResponseDTO>.Failure(
                    $"Invalid operational time: Allowed range between {Rules.START_TIME:t} and {Rules.END_TIME:t}",
                    StatusCodes.BadRequest
                );
            }

            //Validation: Ensure EstimatedDruation is valid period
            if(request.EstimatedDuration < Rules.MIN_ALLOWED_LEG_TIME || request.EstimatedDuration > Rules.MAX_ALLOWED_LEG_TIME)
            {
                return ApiResponse<AddScheduleRulesResponseDTO>.Failure(
                    $"Invalid journey duration: Estimated travel lenght must be between {Rules.MIN_ALLOWED_LEG_TIME.TotalMinutes} minutes and {Rules.MAX_ALLOWED_LEG_TIME.TotalHours} hours",
                    StatusCodes.BadRequest);
            }

            //parameters
            var duplicateCheckParams = new Dictionary<string, object>
            {
                { nameof(ScheduleRules.RouteId), request.RouteId },
                { nameof(ScheduleRules.DayOfWeek), (int)request.DayOfWeek },
                { nameof(ScheduleRules.ScheduledDepartureTime), request.ScheduledDepartureTime }
            };
            int querylimit = 1;

            //check that day of week, departure time and routeID combo does not exist in db already
            var existingRule = await _rulesQueryRepository.FindByMultipleFieldsAsync(duplicateCheckParams, querylimit);
            if(existingRule.Any())
            {
                return ApiResponse<AddScheduleRulesResponseDTO>.Failure(
                    $"A schedule rule exists for this route on this day at {request.ScheduledDepartureTime}.",
                    StatusCodes.BadRequest
                );
            }

            //Insert new SceduleRule
            var scheduleRule = new ScheduleRules
            {
                RouteId = request.RouteId,
                DayOfWeek = request.DayOfWeek,
                ScheduledDepartureTime = request.ScheduledDepartureTime,
                EstimatedDuration = request.EstimatedDuration
            };
            await _rulesCommandRepository.AddAsync(scheduleRule);

            return ApiResponse<AddScheduleRulesResponseDTO>.Success(
                "Schedule Rule added",
                new AddScheduleRulesResponseDTO
                {
                    DayOfWeek = scheduleRule.DayOfWeek.ToString(),
                    DepartureTime = scheduleRule.ScheduledDepartureTime.ToShortTimeString(),
                    RouteName = route.RouteName                    
                });
        }
        catch (Exception e)
        {
            return ApiResponse<AddScheduleRulesResponseDTO>.Failure(
                e.Message, StatusCodes.ServerError);
        }
    }

    //Service to add schedule
    //Supposed to run in the background every day to create the daily schedule
    
    public async Task<ApiResponse> AddScheduleJob()
    {
        try
        {
            DateTime today = DateTime.UtcNow.Date;
            
            //check there are no entries for today in the schedule db
            var existingCounter = await _scheduleQueryRepository.FindByCriteriaAsync(nameof(Schedule.DateOfDeparture), today.ToString("yyyy-MM-dd"));
            if (existingCounter != null)
                return ApiResponse.Failure($"Daily schedule generation skipped: Schedule already exists for {today.Date}");
            
            //Get schedule rules entries where the day matches todays date
            var currentDayofweek = _generalHelpers.MapToWorkingDaysEnum(today.DayOfWeek.ToString()).Data;
            var searchParams = new Dictionary<string, object>
            {
                { nameof(ScheduleRules.DayOfWeek), currentDayofweek },
                { nameof(ScheduleRules.IsActive), true }
            };

            //get the rules that match the execution day
            var rules = (await _rulesQueryRepository.FindByMultipleFieldsAsync(searchParams, null)).ToList();
            if (rules.Count == 0)
            {
                return ApiResponse.Failure("No rules found for this date");
            }

            //Transaction
            using var transaction = _scheduleCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //loop through the rules and insert into as necessary
                foreach (var rule in rules)
                {
                    //get their routeID price
                    var route = await _routQueryRepository.FindByIdAsync(rule.RouteId);

                    var schedule = new Schedule
                    {
                        RouteId = rule.RouteId,
                        DateOfDeparture = DateOnly.FromDateTime(today),
                        DepartureTime = rule.ScheduledDepartureTime,
                        AvailableSeats = 0,
                        Price = route.Price,
                        ArrivalTime = rule.ScheduledDepartureTime.Add(rule.EstimatedDuration, out int wrappedDays),
                        Status = ScheduleStatus.Pending,
                        CreatedFromTemplateID = rule.Id
                    };
                    await _scheduleCommandRepository.AddWithOpenDBTransaction(schedule, transaction);
                }

                _scheduleCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success($"Successfully generated {rules.Count} schedules for {today:d}");
            }
            catch (Exception e)
            {
                if (!isCommitted)
                {
                    _scheduleCommandRepository.RollbackTransaction(transaction);
                }
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    //get schedules for the day
    public async Task<ApiResponse<List<GetSchedulesForDayResponseDTO>>> GetScheduleForToday()
    {
        try
        {
            DateTime today = DateTime.UtcNow.Date;
            var schedules = await _scheduleQueryRepository.GetAllByCriteriaAsync(nameof(Schedule.DateOfDeparture), today.ToString("yyyy-MM-dd"));
            if (!schedules.Any())
            {
                return ApiResponse<List<GetSchedulesForDayResponseDTO>>.Failure(
                    $"No schedules found for this day: {today}", 
                    StatusCodes.BadRequest);
            }

            var responseDataList = schedules.Select(s => new GetSchedulesForDayResponseDTO
            {
                RouteId = s.RouteId,
                ArrivalTime = s.ArrivalTime,
                DepartureTime = s.DepartureTime,
                Status = s.Status
            }).ToList();

            return ApiResponse<List<GetSchedulesForDayResponseDTO>>.Success(
                "Schedules retrieved successfully", 
                responseDataList);
        }
        catch (Exception e)
        {
            return ApiResponse<List<GetSchedulesForDayResponseDTO>>.Failure(
                e.Message, 
                StatusCodes.ServerError);
        }
    }

    //assign buses to schedule
    public async Task<ApiResponse> AddBusToScheduleForTodayJob()
    {
        try
        {            
            //get all schedules for today
            DateTime today = DateTime.UtcNow.Date;
            var searchParams = new Dictionary<string, object>
            {
                { nameof(Schedule.Status), ScheduleStatus.Pending },
                { nameof(Schedule.DateOfDeparture), today.ToString("yyyy-MM-dd") }
            };
            var schedules = await _scheduleQueryRepository.FindByMultipleFieldsAsync(searchParams, null);
            if (!schedules.Any())
            {
                return ApiResponse<List<GetSchedulesForDayResponseDTO>>.Failure(
                    $"No schedules found for this day: {today}", 
                    StatusCodes.BadRequest);
            }

            //get all busIds already assigned to schedules for today
            var assignedBusIds = schedules.Where(s=> s.BusId != null).Select(s=>s.BusId.Value).ToList();

            //using the assignedBusIds, get all buses that are available (have drivers) and are not in the assignedBusIds list
            var allAvailableBuses = await _busQueryRepository.GetAllByCriteriaAsync(nameof(Bus.DriverAssigned), true.ToString());
            if(!allAvailableBuses.Any())
            {
                return ApiResponse.Failure("No available buses found");
            }
            
            var availableBusesToAssign = allAvailableBuses.Where(b=>!assignedBusIds.Contains(b.Id)).ToList();

            //Now get only schedules for today with no bus assigned
            var schedulesWithoutBus = schedules.Where(s=>s.Status == ScheduleStatus.Pending && s.BusId == null).ToList();
            if (!schedulesWithoutBus.Any())
                return ApiResponse.Failure("No unassigned schedule today");

            //ensure number of availableBuses is equal or greater than the number of schedules for today
            if(availableBusesToAssign.Count < schedulesWithoutBus.Count)
            {
                return ApiResponse.Failure(
                    $"Not enough available buses to assign to schedules: Available Buses: {availableBusesToAssign.Count}, Schedules without bus: {schedulesWithoutBus.Count}"
                    );
            }

            //auto assign buses to schedules
            using var transaction = _scheduleCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                var count = schedulesWithoutBus.Count;
                Schedule? schedule = null;
                for(int i = 0; i<count; i++)
                {
                    schedule = schedulesWithoutBus[i];

                    schedule.AvailableSeats = (int)availableBusesToAssign[i].SeatCapacity;
                    schedule.BusId = availableBusesToAssign[i].Id;
                    schedule.Status = ScheduleStatus.Scheduled;

                    await _scheduleCommandRepository.UpdateWithOpenDbTransactionAsync(schedule, transaction);
                    //await _customCommandRepository.UpdateAssignBusesToScheduleAsync(schedulesWithoutBus[i].Id, availableBusesToAssign[i], transaction);
                }
                _scheduleCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success($"{count} schedules have been assigned Buses");
            }
            catch (Exception e)
            {
                if (!isCommitted)
                    _scheduleCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    //unassign buses from completed/cancelled schedules

    //cancel a schedule
    public async Task<ApiResponse> CancelSchedule(CancelScheduleRequestDTO request)
    {
        try
        {
            //ensure ID exists
            var exists = await _scheduleQueryRepository.FindByCriteriaAsync(nameof(Schedule.Id),request.Id.ToString());
            if (exists == null)
                return ApiResponse.Failure(ErrorMessages.SCHEDULE_NOT_FOUND);

            switch (exists.Status)
            {
                case ScheduleStatus.Completed:
                case ScheduleStatus.Expired:
                case ScheduleStatus.Cancelled:
                case ScheduleStatus.OnRoute:
                    return ApiResponse.Failure("Cannot cancel a schedule that is completed, expired, on-route, or already cancelled");
            }

            //change status to cancelled for schedule
            exists.Status = ScheduleStatus.Cancelled;
            await _scheduleCommandRepository.UpdateAsync(exists);

            return ApiResponse.Success("Status changed to Cancelled");
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }
}