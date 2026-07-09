namespace BusBooking.Models.DTO.RequestDTOs;

public class CustomerWalletTopUpRequestDTO
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Password { get; set; }
}