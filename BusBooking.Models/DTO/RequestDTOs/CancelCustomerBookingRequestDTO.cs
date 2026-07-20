namespace BusBooking.Models.DTO.RequestDTOs;

public class CancelCustomerBookingRequestDTO
{
    public string Token {get; set; }
    public int BookingId {get; set; }
}