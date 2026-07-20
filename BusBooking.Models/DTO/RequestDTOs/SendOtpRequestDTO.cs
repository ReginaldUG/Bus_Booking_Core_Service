using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class SendOtpRequestDTO
{
    public required string Name {get; set; }
    [EmailAddress, Required]
    public required string EmailAddress {get; set; }
}