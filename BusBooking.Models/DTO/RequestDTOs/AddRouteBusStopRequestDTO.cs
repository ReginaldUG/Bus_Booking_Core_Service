using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AddRouteBusStopRequestDTO
{
    [Required] 
    public int RouteId { get; set; }
    [Required]
    public int BusStopId { get; set; }
}