namespace BusBooking.Models.DTO.RequestDTOs;

public class VerifyOtpRequestDTO
{
    public string Email { get; set; }
    public int Code { get; set; }
}