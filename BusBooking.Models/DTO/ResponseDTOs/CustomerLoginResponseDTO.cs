namespace BusBooking.Models.DTO.ResponseDTOs;

public class CustomerLoginResponseDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public AccountStatus Status { get; set; }   
    
}