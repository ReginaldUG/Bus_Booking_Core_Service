using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AssignBusToDriverRequestDTO
{
    [Required, StringLength(8)]
    public string PlateNumber { get; set; }
    [Required]
    public int DriverId { get; set; }
}