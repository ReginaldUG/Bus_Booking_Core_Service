namespace BusBooking.Core.Constants;

public class Rules
{
    public const int MIN_DRIVER_AGE = 25;
    public const int MIN_CUSTOMER_AGE = 18;
    public const int MAX_CUSTOMER_AGE = 100;
    public const int MAX_DRIVER_AGE = 70;

    public const decimal MIN_ROUTE_PRICE = 2000;
    public const decimal REFUND_PERCENT = 0.9m;

    //TRIP TIME DURATION RULES
    public static readonly TimeOnly START_TIME = new TimeOnly(7, 0);
    public static readonly TimeOnly END_TIME = new TimeOnly(21, 0);
    public static readonly TimeSpan MAX_ALLOWED_LEG_TIME = TimeSpan.FromHours(3);
    public static readonly TimeSpan MIN_ALLOWED_LEG_TIME = TimeSpan.FromMinutes(5);


    public const string CANCELLED_BY_CUSTOMER = "customer";
    public const string CANCELLED_BY_DRIVER = "driver";
}