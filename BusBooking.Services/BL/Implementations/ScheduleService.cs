using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBooking.Services.Helpers;

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
        IQueryRepository<ScheduleRules> rulesQueryRepository, 
        IQueryRepository<Bus> busQueryRepository, 
        IQueryRepository<Route> routQueryRepository,
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
                        DateOfDeparture = DateTime.UtcNow.Date,
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
            return ApiResponse.Failure($"Crash details: {e.Message} | Trace: {e.StackTrace}");
        }
    }

    public async Task<ApiResponse> AssignBusToSchedule (AssignBusToScheduleRequestDTO request)
    {
        try
        {
            //check that bus exists, has driver (is active)
            var busParams = new Dictionary<string, object>
            {
                { nameof(Bus.Id), request.BusId.ToString() },
                { nameof(Bus.Status), BusStatus.Active }
            };
            var busExists = (await _busQueryRepository.FindByMultipleFieldsAsync(busParams, null)).FirstOrDefault();
            if (busExists == null)
                return ApiResponse.Failure("Bus not valid");
            
            //check that bus is not assinged to any active schedule for today
            var busScheduleParams = new Dictionary<string, object>
            {
                { nameof(Schedule.BusId), busExists.Id.ToString() },
                { nameof(Schedule.DateOfDeparture), DateTime.UtcNow.Date.ToString("yyyy-MM-dd") },
                { nameof(Schedule.Status), ScheduleStatus.Pending }
            };
            var busInSchedule = await _scheduleQueryRepository.FindByMultipleFieldsAsync(busScheduleParams, null);
            if (busInSchedule.Any())
                return ApiResponse.Failure("Bus already assigned to seperate schedule");
            

            //check thaat schedule exists and is pending
            var scheduleParams = new Dictionary<string, object>
            {
                { nameof(Schedule.Id), request.ScheduleId.ToString() },
                { nameof(Schedule.DateOfDeparture), DateTime.UtcNow.Date.ToString("yyyy-MM-dd") },
                { nameof(Schedule.BusId), "null"},
                { nameof(Schedule.Status), ScheduleStatus.Pending }
            };
            var scheduleExist = (await _scheduleQueryRepository.FindByMultipleFieldsAsync(scheduleParams, null)).FirstOrDefault();
            if (scheduleExist == null)
                return ApiResponse.Failure("Schedule not valid");            
            
            DateTime scheduledDateTimeLocal = scheduleExist.DateOfDeparture.Date.Add(scheduleExist.DepartureTime.ToTimeSpan());
            scheduledDateTimeLocal = DateTime.SpecifyKind(scheduledDateTimeLocal, DateTimeKind.Local);
            if (DateTime.Now.AddMinutes(10) > scheduledDateTimeLocal)
            {
                return ApiResponse.Failure("Schedule no longer valid for bus assignment due to departure timeframe restrictions.");
            }
            
            //Assign Bus to schedule
            scheduleExist.BusId = busExists.Id;
            scheduleExist.Status = ScheduleStatus.Scheduled;
            scheduleExist.AvailableSeats = (int)busExists.SeatCapacity;

            await _scheduleCommandRepository.UpdateAsync(scheduleExist);

            return ApiResponse.Success($"Bus '{busExists.PlateNumber}' assigned to schedule");
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
                $"Crash details: {e.Message} | Trace: {e.StackTrace}", 
                StatusCodes.BadRequest);
        }
    }

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