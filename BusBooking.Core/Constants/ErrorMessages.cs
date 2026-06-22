namespace BusBooking.Core.Constants;

public class ErrorMessages
{
    public const string INVALID_CREDENTIALS = "Invalid Credentials";
    public const string DUPLICATE_CUSTOMER_FOUND = "User with these credentials already exist";
    public const string DUPLICATE_DRIVER_FOUND = "Driver with these credentials already exist";
    public const string DUPLICATE_PHONE_NUMBER_FOUND = "User with this number already exists";
    public const string DUPLICATE_ROUTE_FOUND = "Route with this name already exists";
    public const string ROUTE_NOT_FOUND = "Route with this name does not exist";
    public const string BUS_NOT_FOUND = "Bus with this number does not exist";
    public const string NO_PENDING_DRIVER = "No drivers waiting on pending";
    public const string INVALID_BUS_TYPE = "Invalid bus type";
    public const string INVALID_PLATE_NUMBER = "Invalid Plate number";
    public const string DUPLICATE_PLATE_NUMBER_FOUND = "Bus with Plate number already exists";
}