namespace BusBooking.Core.Constants;

public class BusStatus
{
    public const string Active = "active";
    public const string Decommissioned = "decommissioned";
    public const string Faulty = "faulty";
    public const string PendingRoute = "pending_route";
    
    //for when bus has route but no driver assigned
    public const string PendingDriver = "pending_driver";
    
}