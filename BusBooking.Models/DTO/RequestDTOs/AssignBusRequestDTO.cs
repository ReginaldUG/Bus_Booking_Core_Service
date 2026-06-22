using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AssignBusRequestDTO
{
    [Required, MinLength(1)]
    public required string RouteName { get; set; }

    [Required, StringLength(8)]
    public required string BusPlateNumber { get; set; }
    
}