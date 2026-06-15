namespace BusBooking.Core.Constants;

public static class AccountStatus
{
    public const string Active = "active";
    public const string NotActive = "notactive";
    public const string Suspended = "suspended";
    public const string Deleted = "deleted";
    
    //For Driver it means pending a bus creation (driver has no bus assigned to them)
    public const string Pending = "pending";
}