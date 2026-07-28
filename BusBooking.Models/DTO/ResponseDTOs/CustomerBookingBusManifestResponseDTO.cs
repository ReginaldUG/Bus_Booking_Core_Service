namespace BusBooking.Models.DTO.ResponseDTOs;

public class CustomerBookingBusManifestResponseDTO
{
    public string FirstName {get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string PickUp { get; set; }
    public string DropOff { get; set; }
    public string AccountStatus { get; set; }
    public bool Paid { get; set; }
}