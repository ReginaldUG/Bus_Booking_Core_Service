namespace BusBooking.Core.Constants;

public class RouteType
{
    public const string Morning = "morning";
    public const string Evening = "evening";

    public static readonly HashSet<string> AllRouteTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        Morning,
        Evening
    };
}