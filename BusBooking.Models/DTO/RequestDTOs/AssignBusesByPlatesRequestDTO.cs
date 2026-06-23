using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AssignBusesByPlatesRequestDTO
{
    [Required]
    public required List<string> PlateNumbers {get; set; }
    [Required, MinLength(1)]
    public required string RouteName { get; set; }
}