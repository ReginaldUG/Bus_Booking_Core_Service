namespace BusBooking.Models.DTO.ResponseDTOs;

public class CreateRouteResponseDTO
{
    public string RouteName {get; set; }
    public string Type{get; set; }
    public TimeOnly DepartureTime { get; set; }
    
}