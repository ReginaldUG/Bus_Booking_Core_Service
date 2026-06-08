using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class CustomerRegisterRequestDTO
{
    [Required, MinLength(1)]
    public required string FirstName { get; set; }
    [Required, MinLength(1)]
    public required string LastName {get; set; }
    [Required, Range(1,120)]
    public required int Age {get; set; }

    [Required, EmailAddress]
    public required string Email {get; set; }
    [Required, MinLength(8)]
    public required string Password { get; set; }
}