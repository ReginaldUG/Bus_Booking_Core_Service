using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models.DTO.RequestDTOs;

public class CreateRouteRequestDTO
{
    [Required, MinLength(1)]
    public string RouteName { get; set; } 
    [Required]
    public decimal Price { get; set; }
}