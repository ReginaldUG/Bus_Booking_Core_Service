namespace BusBooking.Models.DTO.RequestDTOs;

public class GetSchedulesForDayResponseDTO
{
    public int RouteId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public string Status { get; set; }
}