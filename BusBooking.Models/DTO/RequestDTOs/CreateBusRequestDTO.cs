using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.ResponseDTOs;

public class CreateBusRequestDTO
{
    [Required]
    public string BusSize { get; set; }
    [Required, StringLength(8)]
    public string PlateNumber { get; set; }
    public string? RouteName { get; set; }

}