namespace BusBooking.Models.DTO.RequestDTOs;

public class GetSchedulesForDayResponseDTO
{
    public int RouteId { get; set; }
    public string RouteName { get; set; }
    public List<string> BusStops { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public string Status { get; set; }
}