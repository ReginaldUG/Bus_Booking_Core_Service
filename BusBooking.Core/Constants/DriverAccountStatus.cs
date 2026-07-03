namespace BusBooking.Core.Constants;

public class DriverAccountStatus
{
    public const string Active = "active";
    public const string Suspended = "suspended";
    public const string Deleted = "deleted";
    
    //For Driver it means pending a bus creation (driver has no bus assigned to them)
    public const string PendingBus = "pending_bus";
}