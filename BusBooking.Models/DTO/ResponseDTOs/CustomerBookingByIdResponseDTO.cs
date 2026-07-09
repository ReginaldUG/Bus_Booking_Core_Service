namespace BusBooking.Models.DTO.ResponseDTOs;

public class CustomerBookingByIdResponseDTO
{
    public int CustomerId {get; set; }
    public decimal Price {get; set; }
    public DateOnly DateOfDeparture { get; set; }
    public TimeOnly DepartureTime {get; set; }
    public bool IsPaid {get; set; }
}