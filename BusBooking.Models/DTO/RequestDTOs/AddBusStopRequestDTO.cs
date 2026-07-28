using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AddBusStopRequestDTO
{
    [Required]
    public string Name { get; set; }
}