namespace BusBooking.Models.DTO.RequestDTOs;

public class CustomerWalletTopUpRequestDTO
{
    public required string Token { get; set; }
    public decimal Amount { get; set; }
}