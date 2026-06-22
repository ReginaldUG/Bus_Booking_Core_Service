namespace BusBooking.Models.DTO.RequestDTOs;

public class GetBusesWithoutRouteResponseDTO
{
    public string PlateNumber { get; set; }
    public BusCapacity BusCapacity { get; set; }
    public bool DriverAssigned { get; set; }    
}