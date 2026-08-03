namespace BusBooking.Models.DTO.ResponseDTOs;

public class GetRouteBusStopsResponseDTO
{
    public string RouteName { get; set; }
    public List<string> BusStops { get; set; }
    
}