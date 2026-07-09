namespace BusBooking.Models.DTO.ResponseDTOs;

public class GetAllActiveScheduleForTodayResponseDTO
{
    public int RouteId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public int RemainingSeats { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
}