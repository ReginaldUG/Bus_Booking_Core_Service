using BusBooking.Core.Constants;

namespace BusBooking.Models.DTO.ResponseDTOs;

public class CustomerListResponseDTO
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string PhoneNumber { get; set; }
    public string Status { get; set; }
    public DateTime LastLogin { get; set; }
}