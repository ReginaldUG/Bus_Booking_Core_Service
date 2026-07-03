namespace BusBooking.Core.Constants;

public class ScheduleStatus
{
    public const string Scheduled = "scheduled";
    public const string Pending = "pending"; //pending bus assignement
    public const string OnRoute = "onroute";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
    public const string Expired = "expired";
}