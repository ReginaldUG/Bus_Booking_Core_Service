using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;

namespace BusBooking.Services.BL.Implementations;

public class BookingService : IBookingService
{
    private readonly ICommandRepository<Booking> _bookingCommandRepository;
    
    private readonly ICommandRepository<Schedule> _scheduleCommandRepository;
    private readonly ICommandRepository<CustomerWallet> _walletCommandRepository;
    private readonly ICommandRepository<CustomerWalletTransactions> _txCommandRepository;
    private readonly IQueryRepository<Booking> _bookingQueryRepository;
    private readonly IQueryRepository<Schedule> _scheduleQueryRepository;
    private readonly IQueryRepository<Bus> _busQueryRepository;
    private readonly IQueryRepository<BusStops> _busStopQueryRepository;
    private readonly IQueryRepository<RouteStops> _routeStopQueryRepository;
    private readonly IQueryRepository<Customer> _customerQueryRepository;
    private readonly IQueryRepository<CustomerWallet> _walletQueryRepository;
    private readonly ITokenService _tokenService;
    
    public BookingService(
        IQueryRepository<Booking> bookingQueryRepository, 
        IQueryRepository<Schedule> scheduleQueryRepository,
        ICommandRepository<Booking> bookingCommandRepository, 
        ICommandRepository<Schedule> scheduleCommandRepository,
        IQueryRepository<Customer> customerQueryRepository, 
        IQueryRepository<CustomerWallet> walletQueryRepository,
        IQueryRepository<Bus> busQueryRepository, 
        IQueryRepository<BusStops> busStopQueryRepository,
        IQueryRepository<RouteStops> routeStopQueryRepository,
        ICommandRepository<CustomerWallet> walletCommandRepository, 
        ICommandRepository<CustomerWalletTransactions> txCommandRepository, ITokenService tokenService)
    {
        _bookingCommandRepository = bookingCommandRepository;
        _scheduleCommandRepository = scheduleCommandRepository;
        _walletCommandRepository = walletCommandRepository;
        _txCommandRepository = txCommandRepository;
        _scheduleQueryRepository = scheduleQueryRepository;
        _bookingQueryRepository = bookingQueryRepository;
        _customerQueryRepository = customerQueryRepository;
        _walletQueryRepository = walletQueryRepository;
        _busQueryRepository = busQueryRepository;
        _routeStopQueryRepository = routeStopQueryRepository;
        _busStopQueryRepository = busStopQueryRepository;

        _tokenService = tokenService;
    }

    //book a bus
    public async Task<ApiResponse<BookScheduleResponseDTO>> BookSchedule(BookScheduleRequestDTO request)
    {
        try
        {
            var verify = await _tokenService.VerifyToken(request.Token);
            if (!verify.Status)
                return ApiResponse<BookScheduleResponseDTO>.Failure(ErrorMessages.INVALID_TOKEN,
                    StatusCodes.BadRequest);
            int customerId = verify.Data.CustomerId;

            //Validation 1: Ensure customer and Schedule records exists
            //Validation 2: Ensure customer account is active
            var customer = await _customerQueryRepository.FindByIdAsync(customerId);
            var schedule = await _scheduleQueryRepository.FindByIdAsync(request.ScheduleId);

            string v1 = customer == null ? "Customer not found" :
                schedule == null ? ErrorMessages.SCHEDULE_NOT_FOUND :
                customer.Status != CustomerAccountStatus.Active ? "Customer Account is not Active" : "pass";
            if (v1 != "pass")
                return ApiResponse<BookScheduleResponseDTO>.Failure(v1, StatusCodes.BadRequest);
            
            //Validation 3: Ensure booking time is least 5 mins away from departure time
            DateTime scheduledDateTimeLocal = schedule.DateOfDeparture.Date.Add(schedule.DepartureTime.ToTimeSpan());
            scheduledDateTimeLocal = DateTime.SpecifyKind(scheduledDateTimeLocal, DateTimeKind.Local);
            if (DateTime.Now.AddMinutes(5) > scheduledDateTimeLocal)
            {
                return ApiResponse<BookScheduleResponseDTO>.Failure("Booking deadline for this schedule elapsed", StatusCodes.BadRequest);
            }

            //Validation 4: Ensure that schedule status is valid
            //Validation 5: Ensure seating Space is Available
            string v4 = schedule.Status != ScheduleStatus.Scheduled ? "Schedule is not available for booking" :
                schedule.AvailableSeats < request.NumberOfSeats ? "All seats are booked" : "pass";
            if (v4 != "pass")
                return ApiResponse<BookScheduleResponseDTO>.Failure(v4, StatusCodes.BadRequest);
            
            //Validation 6: Prevent booking duplicate slots for this customer on the same schedule ride
            var searchParam = new Dictionary<string, object>
            {
                { nameof(Booking.CustomerId), customerId },
                { nameof(Booking.ScheduleId), request.ScheduleId }
            };
            var duplicateEntry = await _bookingQueryRepository.FindByMultipleFieldsAsync(searchParam, null);
            if(duplicateEntry.Any())
                return ApiResponse<BookScheduleResponseDTO>.Failure("Customer already has a booking for this schedule", StatusCodes.BadRequest);
            
            //Validation 7: Ensure customer wallet has enough to pay for schedule
            var cWallet = await _walletQueryRepository.FindByCriteriaAsync(nameof(CustomerWallet.CustomerId), customerId.ToString());
            string cm = cWallet == null ? "Customer Wallet not found" :
                cWallet.Balance < schedule.Price ? "Insufficient Wallet Balance" : "good";
            if (cm != "good")
                return ApiResponse<BookScheduleResponseDTO>.Failure(cm, StatusCodes.BadRequest);
                
            //Validation 8: Ensure Pick up and drop off are not the same values
            if(request.PickUpStopId == request.DropOffStopId)
                return ApiResponse<BookScheduleResponseDTO>.Failure("Pick Up and Drop Off cannot be the same location", StatusCodes.BadRequest);
            
            //Validation 9: Ensure Pick Up and Drop off exist
            var pickUp = await _busStopQueryRepository.FindByIdAsync(request.PickUpStopId);
            var dropOff = await _busStopQueryRepository.FindByIdAsync(request.DropOffStopId);
            string m = pickUp == null ? "Invalid PickUp location" : dropOff == null ? "Invalid DropOff location" : "good";
            if (m != "good")
                return ApiResponse<BookScheduleResponseDTO>.Failure(m, StatusCodes.BadRequest);
            
            //Validation 10 : Ensure Pick up and Drop off belong to selected schedule RouteID
            var sp1 = new Dictionary<string, object>
            {
                { nameof(RouteStops.RouteId), schedule.RouteId },
                { nameof(RouteStops.BusStopId), request.PickUpStopId }
            };
            var sp2 = new Dictionary<string, object>
            {
                { nameof(RouteStops.RouteId), schedule.RouteId },
                { nameof(RouteStops.BusStopId), request.DropOffStopId }
            };
            var pickupRouteCheck = (await _routeStopQueryRepository.FindByMultipleFieldsAsync(sp1, null)).FirstOrDefault();
            if (pickupRouteCheck == null)
                return ApiResponse<BookScheduleResponseDTO>.Failure("PickUp not set for Route", StatusCodes.BadRequest);
            
            var dropoffRouteCheck = (await _routeStopQueryRepository.FindByMultipleFieldsAsync(sp2, null)).FirstOrDefault();
            if (dropoffRouteCheck == null)
                return ApiResponse<BookScheduleResponseDTO>.Failure("DropOff not set for Route", StatusCodes.BadRequest);

            //Getting bus oject for response data
            if (schedule.BusId == null)
            {
                return ApiResponse<BookScheduleResponseDTO>.Failure(
                    "A physical bus must be assigne first to the schedule", StatusCodes.BadRequest);
            }
            var bus = await _busQueryRepository.FindByCriteriaAsync(nameof(Bus.Id), schedule.BusId.ToString());
            
            if (bus == null)    
                return ApiResponse<BookScheduleResponseDTO>.Failure("Bus not found for schedule", StatusCodes.BadRequest);
            
            //TRANSACTION OPERATIONS
            using var transaction = _bookingCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //Update schedule seat capacity
                schedule.AvailableSeats--;
                await _scheduleCommandRepository.UpdateWithOpenDbTransactionAsync(schedule, transaction);

                //Update customer wallet
                cWallet.Balance -= schedule.Price;
                cWallet.UpdatedAt = DateTime.UtcNow;
                await _walletCommandRepository.UpdateWithOpenDbTransactionAsync(cWallet, transaction);

                //Add the new Booking entry
                var booking = new Booking
                {
                    CustomerId = customerId,
                    ScheduleId = request.ScheduleId,
                    Price = schedule.Price,
                    PickUpStopId = request.PickUpStopId,
                    DropOffStopId = request.DropOffStopId,
                    IsPaid = true
                };
                var newBooking = await _bookingCommandRepository.AddWithOpenDBTransaction(booking, transaction);

                //Update customer wallet transaction table
                var walletTx = new CustomerWalletTransactions
                {
                    CustomerWalletId = cWallet.Id,
                    Type = TransactionType.Debit,
                    Amount = schedule.Price,
                    Narration = $"Booking for Schedule: ID={schedule.Id}, Date={DateTime.UtcNow}"
                };
                var walletTransaction = await _txCommandRepository.AddWithOpenDBTransaction(walletTx, transaction);

                //Save updates
                _bookingCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse<BookScheduleResponseDTO>.Success(
                    "Ride has been Booked",
                    new BookScheduleResponseDTO
                    {
                        ScheduleID = schedule.Id,
                        DepartureTime = schedule.DepartureTime,
                        ArrivaleTime = schedule.ArrivalTime,
                        BusPlateNumber = bus.PlateNumber,
                        CustomerName = $"{customer.FirstName} {customer.LastName}"
                    });
            }
            catch (Exception)
            {
                if (!isCommitted)
                    _bookingCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse<BookScheduleResponseDTO>.Failure($"Crash details: {e.Message} | Trace: {e.StackTrace}", StatusCodes.ServerError);
        }
    }

    //cancel customer booking
    public async Task<ApiResponse> CancelCustomerBooking (CancelCustomerBookingRequestDTO request)
    {
        try
        {
            //check customer token and get customer ID
            var verify = await _tokenService.VerifyToken(request.Token);            
            if (!verify.Status)
                return ApiResponse.Failure(ErrorMessages.INVALID_TOKEN);
            int customerId = verify.Data.CustomerId;

            //verify booking exists, is assigned to customer, has not yet departed
            var booking = await _bookingQueryRepository.FindByCriteriaAsync(nameof(Booking.Id),request.BookingId.ToString());
            if (booking == null)
                return ApiResponse.Failure("Booking not Found");
            
            var bookingSchedule = await _scheduleQueryRepository.FindByCriteriaAsync(nameof(Schedule.Id), booking.ScheduleId.ToString());
            if (bookingSchedule == null)
                return ApiResponse.Failure("Booking invalid: Schedule not found");
            
            DateTime scheduledDateTimeLocal = bookingSchedule.DateOfDeparture.Date.Add(bookingSchedule.DepartureTime.ToTimeSpan());
            scheduledDateTimeLocal = DateTime.SpecifyKind(scheduledDateTimeLocal, DateTimeKind.Local);

            string message = booking.Completed ? "Cannot cancel a completed booking" :
                booking.CustomerId != customerId ? "Booking is not tied to customer" :
                bookingSchedule.Status != ScheduleStatus.Scheduled ? "Schedule not valid to cancel" :
                DateTime.Now.AddMinutes(5) > scheduledDateTimeLocal ? "Cancellation period has elapsed" : "null";
            if (message != "null")
                return ApiResponse.Failure(message);
            
            var wallet = await _walletQueryRepository.FindByCriteriaAsync(nameof(CustomerWallet.CustomerId),customerId.ToString());
            if (wallet == null)
                return ApiResponse.Failure("Wallet not found");
            
            using var transaction = _bookingCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //Cancel booking
                booking.IsCancelled = true;
                booking.CancelledBy = Rules.CANCELLED_BY_CUSTOMER;
                await _bookingCommandRepository.UpdateWithOpenDbTransactionAsync(booking, transaction);

                //Update schedule seats
                bookingSchedule.AvailableSeats++;
                await _scheduleCommandRepository.UpdateWithOpenDbTransactionAsync(bookingSchedule, transaction);

                //Update Customer Wallet if paid
                if (booking.IsPaid)
                {
                    decimal refund = Rules.REFUND_PERCENT * booking.Price;      //they get 90 percent of the price refunded

                    wallet.Balance += refund;
                    wallet.UpdatedAt = DateTime.UtcNow;
                    await _walletCommandRepository.UpdateWithOpenDbTransactionAsync(wallet, transaction);

                    //Insert new Wallet Transaction
                    var tx = new CustomerWalletTransactions
                    {
                        CustomerWalletId = wallet.Id,
                        Amount = refund,
                        Type = TransactionType.Credit,
                        Narration = $"Refund: NGN{refund} for Cancelled Booking ID={booking.Id}"
                    };
                    var newTx = await _txCommandRepository.AddWithOpenDBTransaction(tx, transaction);
                }

                _bookingCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                string response = booking.IsPaid ? "Booking Cancelled, Refund processed" : "Booking Cancelled";

                return ApiResponse.Success(response);
            }
            catch (Exception)
            {
                if(!isCommitted)
                    _bookingCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    //view customer bookings for today by Id
    public async Task<ApiResponse<List<CustomerBookingByIdResponseDTO>>> GetCustomerBookingById (CustomerBookingByIdRequestDTO request)
    {
        try
        {
            var searchParams = new Dictionary<string, object>
            {
                {nameof(Booking.CustomerId), request.CustomerId},
                {nameof(Booking.Completed), false}
            };
            var bookings = await _bookingQueryRepository.FindByMultipleFieldsAsync(searchParams, null);
            if (!bookings.Any())
                return ApiResponse<List<CustomerBookingByIdResponseDTO>>.Failure("Customer has no bookings",
                    StatusCodes.BadRequest);
            
            var customerBooking = bookings.ToList();
            var returnDataList = new List<CustomerBookingByIdResponseDTO>();

            var scheduleIds = bookings.Select(b => b.ScheduleId.ToString());
            //get all schedules based on id
            var bookSchedule = await _scheduleQueryRepository.FindAllByMultipleValuesAsync(nameof(Schedule.Id), scheduleIds);
            var scheduleList = bookSchedule.ToList();

            for (int i = 0; i < customerBooking.Count; i++)
            {
                CustomerBookingByIdResponseDTO response = new CustomerBookingByIdResponseDTO
                {
                    CustomerId = customerBooking[i].CustomerId,
                    DateOfDeparture = DateOnly.FromDateTime(scheduleList[i].DateOfDeparture),
                    DepartureTime = scheduleList[i].DepartureTime,
                    IsPaid = customerBooking[i].IsPaid,
                    Price = customerBooking[i].Price
                };
                returnDataList.Add(response);
            }

            return ApiResponse<List<CustomerBookingByIdResponseDTO>>.Success(
                "Booking retrieved",
                returnDataList);
        }
        catch (Exception e)
        {
            return ApiResponse<List<CustomerBookingByIdResponseDTO>>.Failure(e.Message, StatusCodes.ServerError);            
        }
    }

    //Generate Manifest for Bus, At Time, On Day
    public async Task<ApiResponse<List<CustomerBookingBusManifestResponseDTO>>> GetCustomerBookingBusManifest(CustomerBookingBusManifestRequestDTO request)
    {
        try
        {
            //check that bus exists
            var bus = await _busQueryRepository.FindByIdAsync(request.BusId);
            if (bus == null)
                return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure(ErrorMessages.BUS_NOT_FOUND,
                    StatusCodes.BadRequest);
            
            //check that bus has schedule for that day at requested time
            //ensure the string is correct format
            var searchParams = new Dictionary<string, object>
            {
                { nameof(Schedule.BusId), request.BusId },
                { nameof(Schedule.DateOfDeparture), request.Day },
                { nameof(Schedule.DepartureTime), request.DepartureTime },
                {nameof(Schedule.Status), ScheduleStatus.Scheduled}
            };
            var schedule = (await _scheduleQueryRepository.FindByMultipleFieldsAsync(searchParams, null)).FirstOrDefault();
            if (schedule == null)
                return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure(
                    ErrorMessages.SCHEDULE_NOT_FOUND, StatusCodes.BadRequest);
            
            //Retrieve all bookings for that scheduleId
            var allBookings =
                await _bookingQueryRepository.GetAllByCriteriaAsync(nameof(Booking.ScheduleId), schedule.Id.ToString());
            if (!allBookings.Any())
                return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure("Bookings not found",
                    StatusCodes.BadRequest);
            var bookings = allBookings.ToList();

            List<CustomerBookingBusManifestResponseDTO> customers = new List<CustomerBookingBusManifestResponseDTO>();
            //Retrieve all customer Info for bookings
            foreach (var booking in bookings)
            {
                var customer =
                    await _customerQueryRepository.FindByCriteriaAsync(nameof(Customer.Id),
                        booking.CustomerId.ToString());
                if(customer==null)
                    return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure("Customer not found", StatusCodes.BadRequest);
                
                //retrieve the bus stops for that customer
                var pickup = await _busStopQueryRepository.FindByIdAsync(booking.PickUpStopId);
                var dropoff = await _busStopQueryRepository.FindByIdAsync(booking.DropOffStopId);
                
                string m = pickup == null ? "Pick Up Stop not found" :
                    dropoff == null ? "Drop Off Stop not found" : "pass";
                if (m != "pass")
                    return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure(m, StatusCodes.BadRequest);

                //fill return list
                var c = new CustomerBookingBusManifestResponseDTO
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    AccountStatus = customer.Status,
                    Age = customer.Age,
                    PickUp = pickup.Name,
                    DropOff = dropoff.Name,
                    Paid = booking.IsPaid
                };
                customers.Add(c);
            }

            return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Success(
                "Manifest Retrieved",
                customers
            );
        }
        catch (Exception e)
        {
            return ApiResponse<List<CustomerBookingBusManifestResponseDTO>>.Failure(e.Message, StatusCodes.ServerError);
        }
    }

}