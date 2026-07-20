namespace BusBooking.Models.DTO.RequestDTOs;

public class CustomerBookingBusManifestRequestDTO
{
    public int BusId { get; set; }
    public string Day { get; set; } //format (yyyy-MM-dd)
    public TimeOnly DepartureTime { get; set; }
}