using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AssignBusesByCountRequestDTO
{
    [Required]
    public required int NumberOfBuses { get; set; }

    [Required, MinLength(1)]
    public required string RouteName { get; set; }    
}