namespace BusBooking.Models.DTO.ResponseDTOs;

public class GetBusListResponseDTO
{
    public int BusId { get; set; }
    public BusCapacity SeatCapacity { get; set; }
    public string PlateNumber {get; set; }
    public string? DriverEmail { get; set; }
    public string Status { get; set; }
}