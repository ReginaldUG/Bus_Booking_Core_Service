namespace BusBooking.Models.DTO.RequestDTOs;

public class EmailVerificationRequestDTO
{
    public string Email {get; set; }
    public int Code { get; set; }
}