namespace BusBooking.Models.DTO.RequestDTOs;

public class DriverRegisterResponseDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Status { get; set; }
    public string? AssignedBus { get; set; }
}