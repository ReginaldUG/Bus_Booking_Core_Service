namespace BusBooking.Models.DTO.ResponseDTOs;

public class CustomerRegisterResponseDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Status { get; set; }
}