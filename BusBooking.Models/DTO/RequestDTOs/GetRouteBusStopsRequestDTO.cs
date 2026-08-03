using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class GetRouteBusStopsRequestDTO
{
    [Required]
    public int RouteId { get; set; }    
}