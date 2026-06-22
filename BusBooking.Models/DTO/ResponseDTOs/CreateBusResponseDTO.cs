namespace BusBooking.Models.DTO.ResponseDTOs;

public class CreateBusResponseDTO
{
    public BusCapacity BusCapacity { get; set; }
    public string Status { get; set; }
    public string PlateNumber {get; set; }
}